import React from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { 
  Users, 
  Activity, 
  TrendingUp, 
  Shield, 
  User, 
  Settings,
  Calendar,
  Clock
} from 'lucide-react';

const Dashboard = () => {
  const { user } = useAuth();

  const stats = [
    {
      name: 'Total Users',
      value: '1,234',
      change: '+12%',
      changeType: 'positive',
      icon: Users,
      color: 'bg-blue-500'
    },
    {
      name: 'Active Sessions',
      value: '89',
      change: '+5%',
      changeType: 'positive',
      icon: Activity,
      color: 'bg-green-500'
    },
    {
      name: 'System Load',
      value: '67%',
      change: '-2%',
      changeType: 'negative',
      icon: TrendingUp,
      color: 'bg-yellow-500'
    },
    {
      name: 'Security Score',
      value: '94',
      change: '+1%',
      changeType: 'positive',
      icon: Shield,
      color: 'bg-red-500'
    }
  ];

  const recentActivity = [
    {
      id: 1,
      user: 'John Doe',
      action: 'Updated profile settings',
      time: '2 minutes ago',
      avatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150&h=150&fit=crop&crop=face'
    },
    {
      id: 2,
      user: 'Jane Smith',
      action: 'Created new user account',
      time: '15 minutes ago',
      avatar: 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=150&h=150&fit=crop&crop=face'
    },
    {
      id: 3,
      user: 'Mike Johnson',
      action: 'Changed system settings',
      time: '1 hour ago',
      avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&h=150&fit=crop&crop=face'
    },
    {
      id: 4,
      user: 'Sarah Wilson',
      action: 'Logged in from new device',
      time: '2 hours ago',
      avatar: 'https://images.unsplash.com/photo-1494790108755-2616b612b786?w=150&h=150&fit=crop&crop=face'
    }
  ];

  const getRoleBasedContent = () => {
    switch (user?.role) {
      case 'Admin':
        return {
          title: 'Admin Dashboard',
          description: 'Full system access and user management capabilities',
          features: [
            'User Management',
            'System Settings',
            'Security Monitoring',
            'Analytics & Reports'
          ]
        };
      case 'User':
        return {
          title: 'User Dashboard',
          description: 'Personal workspace and basic system access',
          features: [
            'Profile Management',
            'Personal Settings',
            'Activity History',
            'Notifications'
          ]
        };
      default:
        return {
          title: 'Dashboard',
          description: 'Welcome to the admin portal',
          features: []
        };
    }
  };

  const content = getRoleBasedContent();

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">{content.title}</h1>
        <p className="mt-1 text-sm text-gray-500">{content.description}</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => (
          <div key={stat.name} className="card">
            <div className="flex items-center">
              <div className={`flex-shrink-0 p-3 rounded-lg ${stat.color} bg-opacity-10`}>
                <stat.icon className={`h-6 w-6 ${stat.color.replace('bg-', 'text-')}`} />
              </div>
              <div className="ml-4 flex-1">
                <p className="text-sm font-medium text-gray-500">{stat.name}</p>
                <p className="text-2xl font-semibold text-gray-900">{stat.value}</p>
              </div>
            </div>
            <div className="mt-4">
              <span className={`inline-flex items-baseline px-2.5 py-0.5 rounded-full text-sm font-medium ${
                stat.changeType === 'positive' 
                  ? 'bg-green-100 text-green-800' 
                  : 'bg-red-100 text-red-800'
              }`}>
                {stat.change}
              </span>
              <span className="ml-2 text-sm text-gray-500">from last month</span>
            </div>
          </div>
        ))}
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Role-based Features */}
        <div className="card">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Available Features</h3>
          <div className="space-y-3">
            {content.features.map((feature, index) => (
              <div key={index} className="flex items-center">
                <div className="flex-shrink-0">
                  <div className="w-2 h-2 bg-primary-500 rounded-full"></div>
                </div>
                <p className="ml-3 text-sm text-gray-700">{feature}</p>
              </div>
            ))}
          </div>
        </div>

        {/* Recent Activity */}
        <div className="card">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Recent Activity</h3>
          <div className="space-y-4">
            {recentActivity.map((activity) => (
              <div key={activity.id} className="flex items-start space-x-3">
                <img
                  className="h-8 w-8 rounded-full"
                  src={activity.avatar}
                  alt=""
                />
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-gray-900">
                    {activity.user}
                  </p>
                  <p className="text-sm text-gray-500">
                    {activity.action}
                  </p>
                </div>
                <div className="flex items-center text-xs text-gray-400">
                  <Clock className="h-3 w-3 mr-1" />
                  {activity.time}
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="card">
        <h3 className="text-lg font-medium text-gray-900 mb-4">Quick Actions</h3>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {user?.role === 'Admin' && (
            <>
              <button className="flex items-center p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors">
                <Users className="h-5 w-5 text-gray-400 mr-3" />
                <span className="text-sm font-medium text-gray-900">Manage Users</span>
              </button>
              <button className="flex items-center p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors">
                <Shield className="h-5 w-5 text-gray-400 mr-3" />
                <span className="text-sm font-medium text-gray-900">Security Settings</span>
              </button>
            </>
          )}
          <button className="flex items-center p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors">
            <User className="h-5 w-5 text-gray-400 mr-3" />
            <span className="text-sm font-medium text-gray-900">View Profile</span>
          </button>
          <button className="flex items-center p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors">
            <Settings className="h-5 w-5 text-gray-400 mr-3" />
            <span className="text-sm font-medium text-gray-900">Settings</span>
          </button>
        </div>
      </div>
    </div>
  );
};

export default Dashboard; 