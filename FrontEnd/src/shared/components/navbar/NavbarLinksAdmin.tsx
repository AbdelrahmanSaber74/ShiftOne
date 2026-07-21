import { Avatar, Button, Flex, Icon, Menu, MenuButton, MenuItem, MenuList, Text, Tooltip, useColorMode, useColorModeValue } from '@chakra-ui/react';
import { useNavigate } from 'react-router-dom';
import { IoMdMoon, IoMdNotificationsOutline, IoMdSunny } from 'react-icons/io';
import { SidebarResponsive } from 'shared/components/sidebar/Sidebar';
import { useAuth } from 'shared/contexts/AuthContext';
import routes from 'routes';
import LanguageSwitcher from './LanguageSwitcher';
import { useTranslation } from 'react-i18next';

interface HeaderLinksProps {
  secondary?: boolean;
}

export default function HeaderLinks({ secondary }: HeaderLinksProps) {
  const { colorMode, toggleColorMode } = useColorMode();
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const { t } = useTranslation();

  const navbarIcon = useColorModeValue('secondaryGray.600', 'secondaryGray.200');
  const menuBg = useColorModeValue('white', 'navy.800');
  const textColor = useColorModeValue('secondaryGray.900', 'white');
  const borderColor = useColorModeValue('gray.200', 'whiteAlpha.100');

  const displayName = user ? `${user.firstName ?? ''} ${user.lastName ?? ''}`.trim() || t('common.shiftOneUser') : t('common.shiftOneUser');
  const firstName = user?.firstName || t('common.userFallback');

  return (
    <Flex w={{ base: '100%', md: 'auto' }} alignItems="center" flexDirection="row" bg={menuBg} flexWrap={secondary ? { base: 'wrap', md: 'nowrap' } : 'nowrap'} p="6px" border="1px solid" borderColor={borderColor} borderRadius="8px" gap="8px">
      <SidebarResponsive routes={routes} />
      <LanguageSwitcher />
      <Tooltip label={t('common.notifications')}>
        <Button variant="ghost" p="0" minW="34px" h="34px" aria-label={t('common.notifications')} isDisabled>
          <Icon h="18px" w="18px" color={navbarIcon} as={IoMdNotificationsOutline as React.ElementType} />
        </Button>
      </Tooltip>
      <Tooltip label={t('common.themeToggle')}>
        <Button variant="ghost" p="0" minW="34px" h="34px" onClick={toggleColorMode} aria-label={t('common.themeToggle')}>
          <Icon h="18px" w="18px" color={navbarIcon} as={(colorMode === 'light' ? IoMdMoon : IoMdSunny) as React.ElementType} />
        </Button>
      </Tooltip>
      <Menu>
        <MenuButton p="0">
          <Avatar _hover={{ cursor: 'pointer' }} color="white" name={displayName} src={user?.imagePath || undefined} bg="brand.700" size="sm" w="34px" h="34px" />
        </MenuButton>
        <MenuList p="0" mt="10px" borderRadius="8px" bg={menuBg} border="1px solid" borderColor={borderColor} overflow="hidden">
          <Flex w="100%" mb="0">
            <Text px="16px" pt="14px" pb="10px" w="100%" borderBottom="1px solid" borderColor={borderColor} fontSize="sm" fontWeight="700" color={textColor}>
              {t('common.hey', { name: firstName })}
            </Text>
          </Flex>
          <Flex flexDirection="column" p="6px">
            <MenuItem borderRadius="6px" px="12px" onClick={() => navigate('/admin/profile')}>
              <Text fontSize="sm">{t('navigation.myProfile')}</Text>
            </MenuItem>
            <MenuItem color="red.500" borderRadius="6px" px="12px" onClick={logout}>
              <Text fontSize="sm">{t('common.logout')}</Text>
            </MenuItem>
          </Flex>
        </MenuList>
      </Menu>
    </Flex>
  );
}