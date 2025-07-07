// contexts/AuthContext.js
import React, { createContext, useState, useEffect } from 'react';

export const AuthContext = createContext();

/** crude client‑side JWT decoder (no signature check) */
const parseJwt = (token) => {
  try {
    const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    const json  = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(json);
  } catch {
    return {};
  }
};

export const AuthProvider = ({ children }) => {
  const [token, setToken] = useState(null);
  const [role,  setRole]  = useState(null);      // ✨ new state

  /** helper to persist + decode */
  const processToken = (tok) => {
    if (!tok) return;
    localStorage.setItem('token', tok);
    setToken(tok);

    const payload = parseJwt(tok);
    // Adjust claim name if your API uses something else
    const roles = payload.role || payload.roles || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    setRole(Array.isArray(roles) ? roles[0] : roles);
  };

  useEffect(() => {
    const saved = localStorage.getItem('token');
    if (saved) processToken(saved);
  }, []);

  const login  = (tok) => processToken(tok);
  const logout = () => {
    localStorage.removeItem('token');
    setToken(null);
    setRole(null);
  };

  return (
    <AuthContext.Provider value={{ token, role, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};
