import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:7100/api', // ⚠️ use your backend port
});

// Automatically attach token if available
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  console.log(token)
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
