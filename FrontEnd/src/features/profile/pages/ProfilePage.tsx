import React from 'react';
import {
  Avatar,
  Badge,
  Box,
  Button,
  Divider,
  Flex,
  Grid,
  GridItem,
  Heading,
  Icon,
  SimpleGrid,
  Stack,
  Text,
  useColorModeValue,
  Wrap,
  WrapItem,
} from '@chakra-ui/react';
import {
  MdRefresh,
  MdEmail,
  MdPhone,
  MdPerson,
} from 'react-icons/md';
import { useTranslation } from 'react-i18next';

import { useAuth } from 'shared/contexts/AuthContext';
import SharedCard from 'shared/components/ui/SharedCard';
import SharedPageHeader from 'shared/components/ui/SharedPageHeader';

interface DetailItemProps {
  icon: React.ReactElement;
  label: string;
  value?: string | number | null;
}

function DetailItem({ icon, label, value }: DetailItemProps) {
  const mutedText = useColorModeValue('secondaryGray.600', 'secondaryGray.400');
  const textColor = useColorModeValue('secondaryGray.900', 'white');
  const borderCol = useColorModeValue('gray.100', 'whiteAlpha.50');
  const iconCol = useColorModeValue('brand.500', 'brand.400');
  const { t } = useTranslation();

  return (
    <Flex
      align="center"
      gap="14px"
      p="14px 18px"
      border="1px solid"
      borderColor={borderCol}
      borderRadius="12px"
      bg={useColorModeValue('gray.50/30', 'whiteAlpha.10')}
    >
      <Flex
        align="center"
        justify="center"
        w="38px"
        h="38px"
        borderRadius="10px"
        bg={useColorModeValue('brand.50', 'whiteAlpha.100')}
        color={iconCol}
      >
        {icon}
      </Flex>
      <Box minW="0">
        <Text fontSize="xs" color={mutedText} fontWeight="600" textTransform="uppercase" letterSpacing="0.5px">
          {label}
        </Text>
        <Text color={textColor} fontWeight="700" mt="2px" fontSize="sm" isTruncated>
          {value || t('profile.notProvided')}
        </Text>
      </Box>
    </Flex>
  );
}

export default function ProfilePage() {
  const { user, refreshProfile, isLoading } = useAuth();
  const textColor = useColorModeValue('secondaryGray.900', 'white');
  const mutedText = useColorModeValue('secondaryGray.600', 'secondaryGray.400');
  const borderColor = useColorModeValue('gray.200', 'whiteAlpha.100');
  const { t } = useTranslation();

  const displayName = [user?.firstName, user?.lastName].filter(Boolean).join(' ') || t('common.shiftOneUser');

  return (
    <Flex direction="column" w="100%">
      <SharedPageHeader
        title={t('navigation.profile')}
        description=""
        action={
          <Button
            leftIcon={<Icon as={MdRefresh as React.ElementType} />}
            variant="brand"
            onClick={refreshProfile}
            isLoading={isLoading}
            w={{ base: '100%', md: 'auto' }}
          >
            {t('profile.refreshProfile')}
          </Button>
        }
      />

      <Grid templateColumns={{ base: '1fr', lg: '300px 1fr' }} gap="24px" w="100%">
        {/* Left Column - Card Profile Overview */}
        <GridItem>
          <SharedCard p="24px" h="100%">
            <Flex direction="column" align="center" textAlign="center" w="100%">
              <Avatar
                size="2xl"
                name={displayName}
                src={user?.imagePath || undefined}
                border="4px solid"
                borderColor="brand.500"
                boxShadow="0px 10px 20px rgba(112, 144, 176, 0.15)"
              />
              <Heading as="h3" size="md" color={textColor} fontWeight="800" mt="18px">
                {displayName}
              </Heading>
              <Text color={mutedText} fontSize="xs" fontWeight="600" mt="4px" maxW="200px" isTruncated>
                {user?.email || t('profile.noPrimaryContact')}
              </Text>

              <Badge
                mt="14px"
                colorScheme={user?.isActive ? 'green' : 'red'}
                variant="subtle"
                borderRadius="6px"
                px="12px"
                py="3px"
                fontSize="xs"
                fontWeight="700"
              >
                {user?.isActive ? t('dashboard.active') : t('dashboard.inactive')}
              </Badge>

              <Divider my="20px" borderColor={borderColor} />

              <Stack spacing="14px" w="100%" align="stretch">
                <Flex align="center" justify="space-between">
                  <Text fontSize="xs" fontWeight="700" color={mutedText}>
                    {t('profile.emailStatus')}
                  </Text>
                  <Badge colorScheme={user?.emailConfirmed ? 'green' : 'yellow'} borderRadius="6px" px="8px" py="2px" fontSize="10px">
                    {user?.emailConfirmed ? t('profile.verified') : t('profile.pendingVerification')}
                  </Badge>
                </Flex>
                <Flex align="center" justify="space-between">
                  <Text fontSize="xs" fontWeight="700" color={mutedText}>
                    {t('profile.phoneStatus')}
                  </Text>
                  <Badge colorScheme={user?.phoneConfirmed ? 'green' : 'yellow'} borderRadius="6px" px="8px" py="2px" fontSize="10px">
                    {user?.phoneConfirmed ? t('profile.verified') : t('profile.pendingVerification')}
                  </Badge>
                </Flex>
              </Stack>
            </Flex>
          </SharedCard>
        </GridItem>

        {/* Right Column - Cards with details and permissions */}
        <GridItem>
          <Stack spacing="24px">
            {/* Card 1 - General Info */}
            <SharedCard p="24px">
              <Heading as="h4" size="sm" color={textColor} fontWeight="800" mb="16px" textTransform="uppercase" letterSpacing="0.5px">
                {t('profile.generalInfo')}
              </Heading>
              <SimpleGrid columns={{ base: 1, md: 2 }} gap="16px">
                <DetailItem icon={<Icon as={MdPerson as React.ElementType} w="18px" h="18px" />} label={t('profile.firstName')} value={user?.firstName} />
                <DetailItem icon={<Icon as={MdPerson as React.ElementType} w="18px" h="18px" />} label={t('profile.lastName')} value={user?.lastName} />
                <DetailItem icon={<Icon as={MdEmail as React.ElementType} w="18px" h="18px" />} label={t('profile.email')} value={user?.email} />
                <DetailItem icon={<Icon as={MdPhone as React.ElementType} w="18px" h="18px" />} label={t('profile.phoneNumber')} value={user?.phoneNumber} />
              </SimpleGrid>
            </SharedCard>

            {/* Card 2 - Security & Permissions */}
            <SharedCard p="24px">
              <Heading as="h4" size="sm" color={textColor} fontWeight="800" mb="16px" textTransform="uppercase" letterSpacing="0.5px">
                {t('profile.accountSecurity')}
              </Heading>

              <Box mb="20px">
                <Text fontSize="xs" fontWeight="700" color={mutedText} mb="8px" textTransform="uppercase" letterSpacing="0.5px">
                  {t('profile.rolesHeader')}
                </Text>
                <Wrap spacing="8px">
                  {(user?.roles || []).map((role) => (
                    <WrapItem key={role}>
                      <Badge colorScheme="brandScheme" variant="solid" borderRadius="6px" px="10px" py="4px" fontSize="xs">
                        {role}
                      </Badge>
                    </WrapItem>
                  ))}
                  {(!user?.roles || user.roles.length === 0) && (
                    <Text fontSize="sm" color={mutedText}>{t('profile.notProvided')}</Text>
                  )}
                </Wrap>
              </Box>

              <Box>
                <Text fontSize="xs" fontWeight="700" color={mutedText} mb="8px" textTransform="uppercase" letterSpacing="0.5px">
                  {t('profile.permissionsHeader')}
                </Text>
                <Wrap spacing="8px">
                  {(user?.permissions || []).map((permission) => (
                    <WrapItem key={permission}>
                      <Badge variant="outline" colorScheme="brandScheme" borderRadius="6px" px="8px" py="3px" fontSize="11px">
                        {permission}
                      </Badge>
                    </WrapItem>
                  ))}
                  {(!user?.permissions || user.permissions.length === 0) && (
                    <Text fontSize="sm" color={mutedText}>{t('profile.notProvided')}</Text>
                  )}
                </Wrap>
              </Box>
            </SharedCard>
          </Stack>
        </GridItem>
      </Grid>
    </Flex>
  );
}
