import React, { useState, useEffect } from 'react';
import api from '../services/api';
import { useNavigate } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';

export default function AdminUploadPage() {
  const nav = useNavigate();
  const [file, setFile] = useState(null);
  const [message, setMessage] = useState('');
  const [uploading, setUploading] = useState(false);
  const [fileKey, setFileKey] = useState(Date.now());
  const [docRole, setDocRole] = useState('hr');

  // Admin-only access check
  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token) {
      nav('/');
      return;
    }

    
    try {
      const decoded = jwtDecode(token);
      const role =  decoded.role ||
          decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
      if (!role || role.toLowerCase() !== 'admin') {
        alert('Access denied. Admins only.');
        nav('/');
      }
    } catch (err) {
      console.error('Token decode failed:', err);
      nav('/');
    }
  }, [nav]);

  const onFileChange = (e) => {
    setFile(e.target.files[0]);
    setMessage('');
  };

  const onUpload = async (e) => {
    e.preventDefault();

    if (!file) {
      setMessage('Please select a file first.');
      return;
    }

    const formData = new FormData();
    formData.append('file', file);

    try {
      setUploading(true);
    await api.post('/documents/upload-to-blob', formData, {
      headers: {
       'Content-Type': 'multipart/form-data',
       'x-document-role': docRole      // 👈 send role tag
      },
    });  
    setMessage('✅ File uploaded successfully!');
    } catch (err) {
      console.error(err);
      setMessage('❌ Upload failed. Please try again.');
    } finally {
      setUploading(false);
    setFile(null);
    setFileKey(Date.now());
    }
  };

  return (
    <div className="container mt-4" style={{ maxWidth: 500 }}>
      <h4 className="mb-4">Admin: Upload Enterprise Document</h4>

      <form onSubmit={onUpload}>
        <div className="mb-3">
          <input
            className="form-control"
            type="file"
            key={fileKey}
            accept=".pdf,.doc,.docx,.txt"
            onChange={onFileChange}
          />
          <br></br>
          <select value={docRole} onChange={e => setDocRole(e.target.value)}
               className="form-select mb-3">
              <option value="hr">HR</option>
              <option value="it">IT</option>
              <option value="finance">Finance</option>
              <option value="general">General</option>
              <option value="admin">Admin</option>              
          </select>

        </div>

        <button
          type="submit"
          className="btn btn-primary w-100"
          disabled={uploading}
        >
          {uploading ? 'Uploading...' : 'Upload'}
        </button>
      </form>

      {message && <div className="alert alert-info mt-3">{message}</div>}
    </div>
  );
}
