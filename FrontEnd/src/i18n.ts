import i18nInstance from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

import translationEN from './locales/en/translation.json';
import translationAR from './locales/ar/translation.json';
import commonEN from './locales/en/common.json';
import commonAR from './locales/ar/common.json';
import authEN from './locales/en/auth.json';
import authAR from './locales/ar/auth.json';
import navigationEN from './locales/en/navigation.json';
import navigationAR from './locales/ar/navigation.json';
import dashboardEN from './locales/en/dashboard.json';
import dashboardAR from './locales/ar/dashboard.json';
import companiesEN from './locales/en/companies.json';
import companiesAR from './locales/ar/companies.json';
import branchesEN from './locales/en/branches.json';
import branchesAR from './locales/ar/branches.json';
import employeesEN from './locales/en/employees.json';
import employeesAR from './locales/ar/employees.json';
import attendanceEN from './locales/en/attendance.json';
import attendanceAR from './locales/ar/attendance.json';
import plansEN from './locales/en/plans.json';
import plansAR from './locales/ar/plans.json';
import subscriptionsEN from './locales/en/subscriptions.json';
import subscriptionsAR from './locales/ar/subscriptions.json';
import usersEN from './locales/en/users.json';
import usersAR from './locales/ar/users.json';
import rolesEN from './locales/en/roles.json';
import rolesAR from './locales/ar/roles.json';
import permissionsEN from './locales/en/permissions.json';
import reportsEN from './locales/en/reports.json';
import permissionsAR from './locales/ar/permissions.json';
import reportsAR from './locales/ar/reports.json';
import profileEN from './locales/en/profile.json';
import profileAR from './locales/ar/profile.json';
import validationEN from './locales/en/validation.json';
import validationAR from './locales/ar/validation.json';
import workSchedulesEN from './locales/en/workSchedules.json';
import workSchedulesAR from './locales/ar/workSchedules.json';

export const i18nNamespaces = [
  'translation',
  'common',
  'auth',
  'navigation',
  'dashboard',
  'companies',
  'branches',
  'employees',
  'attendance',
  'plans',
  'subscriptions',
  'users',
  'roles',
  'permissions',
  'reports',
  'profile',
  'validation',
  'workSchedules',
] as const;

const resources = {
  en: {
    translation: translationEN,
    common: commonEN,
    auth: authEN,
    navigation: navigationEN,
    dashboard: dashboardEN,
    companies: companiesEN,
    branches: branchesEN,
    employees: employeesEN,
    attendance: attendanceEN,
    plans: plansEN,
    subscriptions: subscriptionsEN,
    users: usersEN,
    roles: rolesEN,
    permissions: permissionsEN,
    reports: reportsEN,
    profile: profileEN,
    validation: validationEN,
    workSchedules: workSchedulesEN,
  },
  ar: {
    translation: translationAR,
    common: commonAR,
    auth: authAR,
    navigation: navigationAR,
    dashboard: dashboardAR,
    companies: companiesAR,
    branches: branchesAR,
    employees: employeesAR,
    attendance: attendanceAR,
    plans: plansAR,
    subscriptions: subscriptionsAR,
    users: usersAR,
    roles: rolesAR,
    permissions: permissionsAR,
    reports: reportsAR,
    profile: profileAR,
    validation: validationAR,
    workSchedules: workSchedulesAR,
  },
};

const i18n = i18nInstance as any;

const applyLanguageDocumentState = (lng: string) => {
  const normalizedLanguage = lng?.startsWith('ar') ? 'ar' : 'en';
  const dir = normalizedLanguage === 'ar' ? 'rtl' : 'ltr';
  document.documentElement.dir = dir;
  document.documentElement.lang = normalizedLanguage;
  document.documentElement.style.fontFamily = normalizedLanguage === 'ar'
    ? "'Cairo', 'Inter', system-ui, sans-serif"
    : "'Inter', system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif";
};

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    ns: i18nNamespaces,
    defaultNS: 'translation',
    fallbackNS: [...i18nNamespaces],
    fallbackLng: 'en',
    supportedLngs: ['en', 'ar'],
    interpolation: { escapeValue: false },
    detection: {
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
    },
  });

i18n.on('languageChanged', applyLanguageDocumentState);
applyLanguageDocumentState(i18n.language || 'en');

export default i18n;