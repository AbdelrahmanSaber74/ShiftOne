import authService from 'features/auth/services/authService';

const profileService = {
  getCurrentProfile: authService.getCurrentUser,
};

export default profileService;
