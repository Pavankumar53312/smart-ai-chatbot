import React, { useContext, useEffect, useState } from 'react';
import { AuthContext } from '../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';

export default function Navbar() {
  const { logout } = useContext(AuthContext);
  const nav = useNavigate();
  const [role, setRole] = useState(null);

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const decoded = jwtDecode(token);
        setRole(
          decoded.role ||
          decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]
        );
      } catch (err) {
        console.error('Invalid token', err);
      }
    }
  }, []);

  const handleLogout = () => {
    logout();
    nav('/');
  };

  return (
    <nav style={styles.navbar}>
      <div style={styles.title}>Smart AI Chatbot</div>
      <div style={styles.links}>
        <a href="/ask" style={styles.link}>Ask</a>

        {role === 'Admin' && (
          <>
            <a href="/admin" style={styles.link}>Upload</a>
            <a href="/admin/docs" style={styles.link}>Documents</a>
          </>
        )}

        <button onClick={handleLogout} style={styles.link}>Logout</button>
      </div>
    </nav>
  );
}

const styles = {
  navbar: {
    backgroundColor: '#333',
    color: 'white',
    padding: '10px 20px',
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center'
  },
  title: {
    fontSize: '18px',
    fontWeight: 'bold'
  },
  links: {
    display: 'flex',
    gap: '20px',
    alignItems: 'center'
  },
  link: {
    color: 'white',
    textDecoration: 'none',
    background: 'none',
    border: 'none',
    cursor: 'pointer',
    fontSize: '14px'
  }
};



// import React, { useContext, useEffect, useState } from 'react';
// import { AuthContext } from '../contexts/AuthContext';
// import { useNavigate } from 'react-router-dom';
// import { jwtDecode } from 'jwt-decode';

// export default function Navbar() {
//   const { logout } = useContext(AuthContext);
//   const nav = useNavigate();
//   const [role, setRole] = useState(null);

//   useEffect(() => {
//     const token = localStorage.getItem('token');
//     if (token) {
//       try {
//         const decoded = jwtDecode(token);
//         setRole(
//           decoded.role ||
//             decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]
//         );
//       } catch (err) {
//         console.error('Invalid token', err);
//       }
//     }
//   }, []);

//   const handleLogout = () => {
//     logout();
//     nav('/');
//   };

//   return (
//     <nav className="bg-gray-800 text-white px-6 py-3 flex items-center justify-between shadow-md">
//       <div className="text-xl font-semibold tracking-wide">
//         Smart AI Chatbot
//       </div>

//       <div className="flex items-center gap-6 text-sm">
//         <a href="/ask" className="hover:underline">
//           Ask
//         </a>

//         {role === 'Admin' && (
//           <>
//             <a href="/admin" className="hover:underline">
//               Upload
//             </a>
//             <a href="/admin/docs" className="hover:underline">
//               Documents
//             </a>
//           </>
//         )}

//         <button onClick={handleLogout} className="hover:underline">
//           Logout
//         </button>
//       </div>
//     </nav>
//   );
// }



// import React, { useContext, useEffect, useState } from 'react';
// import { AuthContext } from '../contexts/AuthContext';
// import { useNavigate } from 'react-router-dom';
// import { jwtDecode } from 'jwt-decode';

// export default function Navbar() {
//   const { logout } = useContext(AuthContext);
//   const nav = useNavigate();
//   const [role, setRole] = useState(null);

//   useEffect(() => {
//     const token = localStorage.getItem('token');
//     if (token) {
//       try {
//         const decoded = jwtDecode(token);
//         setRole(decoded.role || decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]);
//         console.log(role);

//       } catch (err) {
//         console.error('Invalid token', err);
//       }
//     }
//   }, []);

//   const handleLogout = () => {
//     logout();
//     nav('/');
//   };

//   return (
//     <nav className="bg-gray-800 text-white p-4 flex justify-between">
//       <div className="font-bold">Smart AI Chatbot</div>
//       <div className="space-x-4">
//         <a href="/ask" className="hover:underline">Ask</a>

//         {role === 'Admin' && (
//           <>
//             <a href="/admin" className="hover:underline">Upload</a>
//             <a href="/admin/docs" className="hover:underline">Documents</a>
//           </>
//         )}

//         <button className="hover:underline" onClick={handleLogout}>Logout</button>
//       </div>
//     </nav>
//   );
// }