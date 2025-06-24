import axios from 'axios';

export const BASE_URL = import.meta.env.VITE_API_BASE_URL;

const axiosApp = axios.create({
  baseURL: BASE_URL,
  // headers: {
  //   'Content-type': 'application/json',
  //   'Access-Control-Allow-Origin': '*',
  // },
});

axiosApp.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    return Promise.reject(error);
  },
);

export default axiosApp;