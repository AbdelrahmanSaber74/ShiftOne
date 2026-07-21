import { Button, useColorModeValue } from '@chakra-ui/react';
import { useTranslation } from 'react-i18next';

export default function LanguageSwitcher() {
  const { t, i18n } = useTranslation();
  const currentLang = i18n.language || 'en';

  const toggleLanguage = () => {
    const nextLang = currentLang.startsWith('en') ? 'ar' : 'en';
    void i18n.changeLanguage(nextLang);
  };

  const btnBg = useColorModeValue('secondaryGray.300', 'whiteAlpha.200');
  const hoverBg = useColorModeValue('secondaryGray.400', 'whiteAlpha.300');
  const textColor = useColorModeValue('navy.700', 'white');

  return (
    <Button
      onClick={toggleLanguage}
      variant="no-hover"
      bg={btnBg}
      color={textColor}
      borderRadius="8px"
      px="12px"
      h="34px"
      fontSize="sm"
      fontWeight="700"
      _hover={{ bg: hoverBg }}
    >
      {t('common.languageSwitch')}
    </Button>
  );
}