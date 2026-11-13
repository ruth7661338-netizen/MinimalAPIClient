import axios from 'axios';
axios.defaults.baseURL = "http://localhost:5117";
axios.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

axios.interceptors.response.use(
  (response) => response,
  (error) => {
    const isLoginRequest = error.config?.url?.includes('/login');

    if (error.response?.status === 401 && !isLoginRequest) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }

    return Promise.reject(error);
  }
);


export default {
  getTasks: async () => {
    const result = await axios.get(`/items`)
    return result.data;
  },

  addTask: async (name) => {
    const result = await axios.post(`/addTask`, { name, isComplete: false })
    return result.data;
  },

  setCompleted: async (id, isComplete) => {
    const result = await axios.put(`/setCompleted/${id}`, { isComplete })
    return result.data;
  },

  deleteTask: async (id) => {
    await axios.delete(`/deleteTask/${id}`)
  },login: async (username, password) => {
  const result = await axios.post("/login", { username, password });
  localStorage.setItem("token", result.data.token);
},

register: async (username, password) => {
  const result = await axios.post("/register", { username, password });
  localStorage.setItem("token", result.data.token);
},

logout: () => {
  localStorage.removeItem("token");
},
  
};