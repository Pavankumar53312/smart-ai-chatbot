import api from './api';

export async function login(email, password) {
  const response = await api.post('/auth/login', { email, password });
  const { token, role } = response.data;

  // Save token and role to localStorage
  localStorage.setItem('token', token);
  localStorage.setItem('role', role);

  return { token, role };
}

export function logout() {
  localStorage.removeItem('token');
  localStorage.removeItem('role');
}
