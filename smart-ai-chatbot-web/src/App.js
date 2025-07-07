import React from 'react';
import { BrowserRouter, Routes, Route, useLocation } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';

import LoginPage from './pages/LoginPage';
import AskPage from './pages/AskPage';
import AdminUploadPage from './pages/AdminUploadPage';
import Navbar from './components/Navbar';
import AdminDocumentsPage from './pages/AdminDocumentPage';

function AppWrapper() {
  const location = useLocation();
  const hideNavbarOn = ['/']; // you can also include "/login" if needed

  return (
    <>
      {!hideNavbarOn.includes(location.pathname) && <Navbar />}

      <Routes>
        <Route path="/" element={<LoginPage />} />
        <Route path="/ask" element={<AskPage />} />
        <Route path="/admin" element={<AdminUploadPage />} />
        <Route path="/admin/docs" element={<AdminDocumentsPage />} />
      </Routes>
    </>
  );
}

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <AppWrapper />
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;



// import React from 'react';
// import { BrowserRouter, Routes, Route } from 'react-router-dom';
// import { AuthProvider } from './contexts/AuthContext';

// import LoginPage from './pages/LoginPage';
// import AskPage from './pages/AskPage';
// import AdminUploadPage from './pages/AdminUploadPage';
// import Navbar from './components/Navbar';
// import AdminDocumentsPage from './pages/AdminDocumentPage';


// function App() {
//   return (
//     <AuthProvider>
//       <BrowserRouter>
//         <Navbar />
//         <Routes>
//           <Route path="/" element={<LoginPage />} />
//           <Route path="/ask" element={<AskPage />} />
//           <Route path="/admin" element={<AdminUploadPage />} />
//           <Route path="/admin/docs" element={<AdminDocumentsPage />} />
//         </Routes>
//       </BrowserRouter>
//     </AuthProvider>
//   );
// }

// export default App;



