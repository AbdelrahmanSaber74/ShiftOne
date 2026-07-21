import type { ReactElement } from 'react';

export type RouteLayout = '/admin' | '/auth';

export interface AppRoute {
  name: string;
  layout: RouteLayout;
  path: string;
  icon?: ReactElement;
  component: ReactElement;
  secondary?: boolean;
  messageNavbar?: string;
  permissions?: string[];
  translationKey?: string;
  breadcrumbKey?: string;
}

