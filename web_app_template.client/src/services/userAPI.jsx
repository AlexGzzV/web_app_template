// Mock user data for user management
const mockUsersData = [
  {
    id: 1,
    email: 'admin@example.com',
    name: 'Admin User',
    role: 'admin',
    status: 'active',
    avatar: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&h=150&fit=crop&crop=face',
    createdAt: '2023-01-15',
    lastLogin: '2023-12-01T10:30:00Z'
  },
  {
    id: 2,
    email: 'manager@example.com',
    name: 'Manager User',
    role: 'manager',
    status: 'active',
    avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&h=150&fit=crop&crop=face',
    createdAt: '2023-02-20',
    lastLogin: '2023-12-01T09:15:00Z'
  },
  {
    id: 3,
    email: 'user@example.com',
    name: 'Regular User',
    role: 'user',
    status: 'active',
    avatar: 'https://images.unsplash.com/photo-1494790108755-2616b612b786?w=150&h=150&fit=crop&crop=face',
    createdAt: '2023-03-10',
    lastLogin: '2023-12-01T08:45:00Z'
  },
  {
    id: 4,
    email: 'john.doe@example.com',
    name: 'John Doe',
    role: 'user',
    status: 'inactive',
    avatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150&h=150&fit=crop&crop=face',
    createdAt: '2023-04-05',
    lastLogin: '2023-11-28T14:20:00Z'
  },
  {
    id: 5,
    email: 'jane.smith@example.com',
    name: 'Jane Smith',
    role: 'manager',
    status: 'active',
    avatar: 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=150&h=150&fit=crop&crop=face',
    createdAt: '2023-05-12',
    lastLogin: '2023-12-01T11:00:00Z'
  }
];

export const userAPI = {
  getUsers: async (page = 1, limit = 10, search = '') => {
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 800));
    
    let filteredUsers = [...mockUsersData];
    
    // Apply search filter
    if (search) {
      filteredUsers = filteredUsers.filter(user => 
        user.name.toLowerCase().includes(search.toLowerCase()) ||
        user.email.toLowerCase().includes(search.toLowerCase())
      );
    }
    
    // Apply pagination
    const startIndex = (page - 1) * limit;
    const endIndex = startIndex + limit;
    const paginatedUsers = filteredUsers.slice(startIndex, endIndex);
    
    return {
      data: {
        users: paginatedUsers,
        total: filteredUsers.length,
        page,
        limit,
        totalPages: Math.ceil(filteredUsers.length / limit)
      }
    };
  },

  getUserById: async (id) => {
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 500));
    
    const user = mockUsersData.find(u => u.id === parseInt(id));
    
    if (!user) {
      throw new Error('User not found');
    }
    
    return { data: user };
  },

  createUser: async (userData) => {
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 1000));
    
    const newUser = {
      id: mockUsersData.length + 1,
      ...userData,
      status: 'active',
      createdAt: new Date().toISOString().split('T')[0],
      lastLogin: null,
      avatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&h=150&fit=crop&crop=face'
    };
    
    mockUsersData.push(newUser);
    
    return { data: newUser };
  },

  updateUser: async (id, userData) => {
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 800));
    
    const userIndex = mockUsersData.findIndex(u => u.id === parseInt(id));
    
    if (userIndex === -1) {
      throw new Error('User not found');
    }
    
    mockUsersData[userIndex] = { ...mockUsersData[userIndex], ...userData };
    
    return { data: mockUsersData[userIndex] };
  },

  deleteUser: async (id) => {
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 600));
    
    const userIndex = mockUsersData.findIndex(u => u.id === parseInt(id));
    
    if (userIndex === -1) {
      throw new Error('User not found');
    }
    
    mockUsersData.splice(userIndex, 1);
    
    return { data: { message: 'User deleted successfully' } };
  },

  updateUserStatus: async (id, status) => {
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 500));
    
    const user = mockUsersData.find(u => u.id === parseInt(id));
    
    if (!user) {
      throw new Error('User not found');
    }
    
    user.status = status;
    
    return { data: user };
  }
}; 