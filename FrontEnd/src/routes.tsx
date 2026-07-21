import { Icon } from '@chakra-ui/react';
import { MdAdminPanelSettings, MdAssessment, MdBusiness, MdEventAvailable, MdHome, MdLock, MdPeople, MdPerson, MdPlace, MdReceipt, MdSchedule, MdSecurity, MdSubscriptions } from 'react-icons/md';
import DashboardPage from 'features/dashboard/pages/DashboardPage';
import ProfilePage from 'features/profile/pages/ProfilePage';
import SignInPage from 'features/auth/pages/SignInPage';
import AttendancePage from 'features/attendance/pages/AttendancePage';
import BranchesPage from 'features/branches/pages/BranchesPage';
import CompaniesPage from 'features/companies/pages/CompaniesPage';
import EmployeesPage from 'features/employees/pages/EmployeesPage';
import PlansPage from 'features/plans/pages/PlansPage';
import PermissionsPage from 'features/permissions/pages/PermissionsPage';
import RolesPage from 'features/roles/pages/RolesPage';
import ReportsPage from 'features/reports/pages/ReportsPage';
import WorkSchedulesPage from 'features/work-schedules/pages/WorkSchedulesPage';
import SubscriptionsPage from 'features/subscriptions/pages/SubscriptionsPage';
import type { AppRoute } from 'shared/types/routes';

const routes: AppRoute[] = [
  {
    name: 'Dashboard',
    translationKey: 'navigation.dashboard',
    breadcrumbKey: 'navigation.dashboard',
    layout: '/admin',
    path: '/default',
    icon: <Icon as={MdHome as React.ElementType} width="20px" height="20px" color="inherit" />,
    component: <DashboardPage />,
  },
  {
    name: 'Plans',
    translationKey: 'navigation.plans',
    breadcrumbKey: 'navigation.plans',
    layout: '/admin',
    path: '/plans',
    icon: <Icon as={MdReceipt as React.ElementType} width="20px" height="20px" color="inherit" />,
    component: <PlansPage />,
    permissions: ['Plans.View'],
  },
  {
    name: 'Companies',
    translationKey: 'navigation.companies',
    breadcrumbKey: 'navigation.companies',
    layout: '/admin',
    path: '/companies',
    icon: <Icon as={MdBusiness as React.ElementType} width="20px" height="20px" color="inherit" />,
    component: <CompaniesPage />,
    permissions: ['Companies.View'],
  },
  {
    name: 'Subscriptions',
    translationKey: 'navigation.subscriptions',
    breadcrumbKey: 'navigation.subscriptions',
    layout: '/admin',
    path: '/subscriptions',
    icon: <Icon as={MdSubscriptions as React.ElementType} width="20px" height="20px" color="inherit" />,
    component: <SubscriptionsPage />,
    permissions: ['Subscriptions.View'],
  },
  {
    name: 'Branches',
    translationKey: 'navigation.branches',
    breadcrumbKey: 'navigation.branches',
    layout: '/admin',
    path: '/branches',
    icon: <Icon as={MdPlace as React.ElementType} width="20px" height="20px" color="inherit" />,
    component: <BranchesPage />,
    permissions: ['Branches.View'],
  },
  {
    name: 'Employees',
    translationKey: 'navigation.employees',
    breadcrumbKey: 'navigation.employees',
    layout: '/admin',
    path: '/employees',
    icon: <Icon as={MdPeople as React.ElementType} width="20px" height="20px" color="inherit" />,
    component: <EmployeesPage />,
    permissions: ['Employees.View'],
  },
  {
    name: 'Work Schedules',
    translationKey: 'navigation.workSchedules',
    breadcrumbKey: 'navigation.workSchedules',
    layout: '/admin',
    path: '/work-schedules',
    icon: <Icon as={MdSchedule as React.ElementType} width="20px" height="20px" color="inherit" />,
    component: <WorkSchedulesPage />,
    permissions: ['WorkSchedules.View'],
  },
  {
    name: 'Attendance',
    translationKey: 'navigation.attendance',
    breadcrumbKey: 'navigation.attendance',
    layout: '/admin',
    path: '/attendance',
    icon: <Icon as={MdEventAvailable as React.ElementType} width="20px" height="20px" color="inherit" />,
    component: <AttendancePage />,
    permissions: ['Attendance.View'],
  },
  {
    name: 'Reports',
    translationKey: 'navigation.reports',
    breadcrumbKey: 'navigation.reports',
    layout: '/admin',
    path: '/reports',
    icon: <Icon as={MdAssessment as React.ElementType} width="20px" height="20px" color="inherit" />,
    component: <ReportsPage />,
    permissions: ['Reports.View'],
  },
  {
    name: 'Roles',
    translationKey: 'navigation.roles',
    breadcrumbKey: 'navigation.roles',
    layout: '/admin',
    path: '/roles',
    icon: <Icon as={MdAdminPanelSettings as React.ElementType} width="20px" height="20px" color="inherit" />,
    component: <RolesPage />,
    permissions: ['Roles.View'],
  },
  {
    name: 'Permissions',
    translationKey: 'navigation.permissions',
    breadcrumbKey: 'navigation.permissions',
    layout: '/admin',
    path: '/permissions',
    icon: <Icon as={MdSecurity as React.ElementType} width="20px" height="20px" color="inherit" />,
    component: <PermissionsPage />,
    permissions: ['Permissions.View'],
  },
  {
    name: 'Profile',
    translationKey: 'navigation.profile',
    breadcrumbKey: 'navigation.profile',
    layout: '/admin',
    path: '/profile',
    icon: <Icon as={MdPerson as React.ElementType} width="20px" height="20px" color="inherit" />,
    component: <ProfilePage />,
  },
  {
    name: 'Sign In',
    translationKey: 'navigation.signIn',
    breadcrumbKey: 'navigation.signIn',
    layout: '/auth',
    path: '/sign-in',
    icon: <Icon as={MdLock as React.ElementType} width="20px" height="20px" color="inherit" />,
    component: <SignInPage />,
  },
];

export default routes;
