import axiosApp from './axiosApp';
import jwtDecode from 'jwt-decode';

export const authAPI = {
  login: async (email, password) => {

    const credentials = btoa(`${email}:${password}`);
    console.log(credentials);
    const config = {
      headers: {
        'Authorization': `Basic ${credentials}`
      }
    };
    const response = await axiosApp.post('/auth/login', null, config);

    if (!(response.status >= 200 && response.status < 300)) {
      throw new Error('Invalid credentials');
    }

    const accessToken = response.data.accessToken;
    const refreshToken = response.data.refreshToken;
    
    return {
      data: {
        accessToken,
        refreshToken
      }
    };
  },

  logout: async () => {
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 500));
    return { data: { message: 'Logged out successfully' } };
  },

  getProfile: async () => {    
    const token = localStorage.getItem('token');
    if (!token) {
      throw new Error('No token found');
    }
    
    try {
      const decoded = JSON.parse(atob(token));
      const user = mockUsers.find(u => u.id === decoded.id);
      
      if (!user) {
        throw new Error('User not found');
      }
      
      return {
        data: {
          email: user.email,
          name: user.firstName + ' ' + user.lastName,
          role: user.role,
          avatar: user.profilePicture
        }
      };
    } catch (error) {
      throw new Error('Invalid token');
    }
  }
}; 