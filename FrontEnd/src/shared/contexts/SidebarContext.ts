import { createContext } from 'react';

export interface SidebarContextValue {
  toggleSidebar: boolean;
  setToggleSidebar: (value: boolean) => void;
}

export const SidebarContext = createContext<SidebarContextValue>({
  toggleSidebar: false,
  setToggleSidebar: () => undefined,
});
