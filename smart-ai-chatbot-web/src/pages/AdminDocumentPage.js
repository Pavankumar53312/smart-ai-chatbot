import React, { useEffect, useState } from "react";
import api from "../services/api";

export default function AdminDocumentsPage() {
  const [docs, setDocs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const fetchDocs = async () => {
    setLoading(true);
    setError("");
    
    try {
      const { data } = await api.get("/documents");
      setDocs(data);
    } catch (err) {
      console.error("Fetch failed", err);
      setError(err.response?.data?.message || "Unable to load documents.");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (fileName) => {
    if (!window.confirm(`Delete ${fileName}?`)) return;
    try {
      await api.delete(`/documents/${fileName}`);
      fetchDocs();
    } catch (err) {
      alert("Delete failed. Please try again.");
    }
  };

  useEffect(() => {
    fetchDocs();
  }, []);

  return (
    <div className="container mt-4">
      <h3 className="mb-3">Uploaded Documents</h3>

      {loading && <p>Loading…</p>}
      {error && <div className="alert alert-warning">{error}</div>}

      {!loading && !error && (
        <table className="table table-bordered">
          <thead>
            <tr>
              <th>File</th>
              <th>Role</th>
              <th># Chunks</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {docs.map((d) => (
              <tr key={d.fileName}>
                <td>{d.fileName}</td>
                <td>{d.role}</td>
                <td>{d.chunks}</td>
                <td>
                  <button
                    className="btn btn-sm btn-danger"
                    onClick={() => handleDelete(d.fileName)}
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}



// import React, { useEffect, useState } from "react";
// import api from "../services/api";

// export default function AdminDocumentsPage() {
//   const [docs, setDocs] = useState([]);
//   const [loading, setLoading] = useState(true);

//   const fetchDocs = async () => {
//     setLoading(true);
//     const { data } = await api.get("/documents");
//     setDocs(data);
//     setLoading(false);
//   };

//   const handleDelete = async (fileName) => {
//     if (!window.confirm(`Delete ${fileName}?`)) return;
//     await api.delete(`/documents/${fileName}`);
//     fetchDocs();
//   };

//   useEffect(() => { fetchDocs(); }, []);

//   if (loading) return <p className="m-4">Loading…</p>;

//   return (
//     <div className="container mt-4">
//       <h3 className="mb-3">Uploaded Documents</h3>
//       <table className="table table-bordered">
//         <thead>
//           <tr>
//             <th>File</th><th>Role</th><th># Chunks</th><th></th>
//           </tr>
//         </thead>
//         <tbody>
//           {docs.map(d => (
//             <tr key={d.fileName}>
//               <td>{d.fileName}</td>
//               <td>{d.role}</td>
//               <td>{d.chunks}</td>
//               <td>
//                 <button className="btn btn-sm btn-danger"
//                         onClick={() => handleDelete(d.fileName)}>
//                   Delete
//                 </button>
//               </td>
//             </tr>
//           ))}
//         </tbody>
//       </table>
//     </div>
//   );
// }
