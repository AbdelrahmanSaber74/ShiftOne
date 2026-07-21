import React, { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Badge,
  Box,
  Button,
  Flex,
  Grid,
  GridItem,
  Heading,
  Icon,
  Progress,
  SimpleGrid,
  Spinner,
  Stack,
  Text,
  useColorModeValue,
  VStack,
} from '@chakra-ui/react';
import {
  MdBusiness,
  MdCloudQueue,
  MdEventAvailable,
  MdGroups,
  MdHistory,
  MdOutlinePhonelinkLock,
  MdPlace,
  MdRefresh,
  MdSubscriptions,
} from 'react-icons/md';
import { useTranslation } from 'react-i18next';

import { useAuth } from 'shared/contexts/AuthContext';
import { isPlatformAdmin } from 'shared/utils/authUtils';
import SharedCard from 'shared/components/ui/SharedCard';
import SharedPageHeader from 'shared/components/ui/SharedPageHeader';
import SharedStatCard from 'shared/components/ui/SharedStatCard';
import { formatDate, formatNumber } from 'shared/utils/localeUtils';

import attendanceService from 'features/attendance/services/attendanceService';
import branchesService from 'features/branches/services/branchesService';
import companiesService from 'features/companies/services/companiesService';
import employeesService from 'features/employees/services/employeesService';
import subscriptionsService from 'features/subscriptions/services/subscriptionsService';
import type { AttendanceRecord, CompanySubscription, Employee } from 'shared/types/api';

interface DashboardCounts {
  companies: number;
  subscriptions: number;
  branches: number;
  employees: number;
  activeEmployees: number;
  todayAttendance: number;
  boundDevices: number;
}

interface ActivityItem {
  id: string;
  title: string;
  detail: string;
  badge: string;
}

const emptyCounts: DashboardCounts = {
  companies: 0,
  subscriptions: 0,
  branches: 0,
  employees: 0,
  activeEmployees: 0,
  todayAttendance: 0,
  boundDevices: 0,
};

function isToday(value?: string) {
  if (!value) return false;
  return value.slice(0, 10) === new Date().toISOString().slice(0, 10);
}

function buildWeeklyTrend(attendance: AttendanceRecord[]) {
  const trend = [0, 0, 0, 0, 0, 0, 0];
  const daysIndexMap: Record<number, number> = { 6: 0, 0: 1, 1: 2, 2: 3, 3: 4, 4: 5, 5: 6 };

  attendance.forEach((record) => {
    if (!record.attendanceDate) return;
    const index = daysIndexMap[new Date(record.attendanceDate).getDay()];
    if (index !== undefined) trend[index] += 1;
  });

  return trend;
}

function ActivityRow({ item }: { item: ActivityItem }) {
  const textColor = useColorModeValue('secondaryGray.900', 'white');
  const mutedText = useColorModeValue('secondaryGray.600', 'secondaryGray.400');
  const borderColor = useColorModeValue('gray.100', 'whiteAlpha.100');

  return (
    <Flex py="12px" gap="12px" align="center" justify="space-between" borderBottom="1px solid" borderColor={borderColor} _last={{ borderBottom: '0' }}>
      <Box minW="0">
        <Text fontSize="sm" fontWeight="800" color={textColor} noOfLines={1}>{item.title}</Text>
        <Text fontSize="xs" color={mutedText} mt="2px" noOfLines={1}>{item.detail}</Text>
      </Box>
      <Badge colorScheme="brandScheme" borderRadius="full" px="10px" py="4px" flexShrink={0}>{item.badge}</Badge>
    </Flex>
  );
}

function WeeklyTrendCard({ trend }: { trend: number[] }) {
  const { t } = useTranslation();
  const textColor = useColorModeValue('secondaryGray.900', 'white');
  const mutedText = useColorModeValue('secondaryGray.600', 'secondaryGray.400');
  const total = trend.reduce((sum, value) => sum + value, 0);
  const max = Math.max(...trend, 1);
  const labels = [
    t('dashboard.weekDays.sat'),
    t('dashboard.weekDays.sun'),
    t('dashboard.weekDays.mon'),
    t('dashboard.weekDays.tue'),
    t('dashboard.weekDays.wed'),
    t('dashboard.weekDays.thu'),
    t('dashboard.weekDays.fri'),
  ];

  return (
    <SharedCard p="24px">
      <Flex justify="space-between" align="flex-start" gap="16px" mb="20px">
        <Box>
          <Text fontSize="xs" fontWeight="800" color={mutedText} textTransform="uppercase">{t('dashboard.attendanceTrend')}</Text>
          <Heading size="sm" color={textColor} mt="4px">{t('dashboard.weeklyTrend')}</Heading>
        </Box>
        <Badge colorScheme="purple" borderRadius="full" px="10px" py="4px">{t('dashboard.totalLogs', { count: total })}</Badge>
      </Flex>

      <SimpleGrid columns={7} gap="10px" alignItems="end" minH="170px">
        {trend.map((value, index) => {
          const height = Math.max(8, Math.round((value / max) * 120));
          return (
            <Flex key={labels[index]} direction="column" align="center" justify="end" gap="8px" minW="0">
              <Text fontSize="xs" fontWeight="800" color={textColor}>{value}</Text>
              <Box w="100%" maxW="34px" h={`${height}px`} borderRadius="8px 8px 4px 4px" bg="brand.500" />
              <Text fontSize="10px" fontWeight="700" color={mutedText} noOfLines={1}>{labels[index]}</Text>
            </Flex>
          );
        })}
      </SimpleGrid>
    </SharedCard>
  );
}

export default function DashboardPage() {
  const { user, refreshProfile, isLoading: authLoading } = useAuth();
  const { t, i18n } = useTranslation();
  const platformAdmin = isPlatformAdmin(user);
  const textColor = useColorModeValue('secondaryGray.900', 'white');
  const mutedText = useColorModeValue('secondaryGray.600', 'secondaryGray.400');
  const surfaceBg = useColorModeValue('gray.50', 'whiteAlpha.50');
  const locale = i18n.language?.startsWith('ar') ? 'ar-EG' : 'en-US';

  const [loading, setLoading] = useState(true);
  const [isOnline, setIsOnline] = useState(true);
  const [latency, setLatency] = useState(0);
  const [counts, setCounts] = useState<DashboardCounts>(emptyCounts);
  const [weeklyTrend, setWeeklyTrend] = useState<number[]>([0, 0, 0, 0, 0, 0, 0]);
  const [activities, setActivities] = useState<ActivityItem[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [subscriptions, setSubscriptions] = useState<CompanySubscription[]>([]);

  const loadDashboardData = useCallback(async () => {
    setLoading(true);
    const startedAt = performance.now();

    try {
      const [branchesRes, employeesRes, attendanceRes, companiesRes, subscriptionsRes] = await Promise.all([
        branchesService.getBranches({ page: 1, pageSize: 500 }),
        employeesService.getEmployees({ page: 1, pageSize: 500 }),
        attendanceService.getAttendance({ page: 1, pageSize: 500 }),
        platformAdmin ? companiesService.getCompanies({ page: 1, pageSize: 1 }) : Promise.resolve(null),
        platformAdmin ? subscriptionsService.getSubscriptions({ page: 1, pageSize: 500 }) : Promise.resolve(null),
      ]);

      const nextBranches = branchesRes.data || [];
      const nextEmployees = employeesRes.data || [];
      const nextAttendance = attendanceRes.data || [];
      const nextSubscriptions = subscriptionsRes?.data || [];
      const todayAttendance = nextAttendance.filter((record) => isToday(record.attendanceDate)).length;

      setEmployees(nextEmployees);
      setSubscriptions(nextSubscriptions);
      setWeeklyTrend(buildWeeklyTrend(nextAttendance));
      setCounts({
        companies: companiesRes?.totalCount ?? 1,
        subscriptions: nextSubscriptions.filter((subscription) => subscription.isActive).length,
        branches: branchesRes.totalCount ?? nextBranches.length,
        employees: employeesRes.totalCount ?? nextEmployees.length,
        activeEmployees: nextEmployees.filter((employee) => employee.isActive).length,
        todayAttendance,
        boundDevices: nextEmployees.filter((employee) => employee.hasBoundDevice).length,
      });
      setActivities(nextAttendance.slice(0, 5).map((record) => ({
        id: record.id,
        title: t('dashboard.activity.checkInTitle', { employee: record.employeeName || t('common.unknown') }),
        detail: `${record.branchName || t('dashboard.unassignedBranch')} Ã‚Â· ${formatDate(record.attendanceDate, locale, { dateStyle: 'medium' })}`,
        badge: record.checkOutAt ? t('dashboard.checkedOut') : t('dashboard.checkedIn'),
      })));
      setLatency(Math.round(performance.now() - startedAt));
      setIsOnline(true);
    } catch (error) {
      console.error('Failed to load dashboard statistics:', error);
      setIsOnline(false);
      setLatency(Math.round(performance.now() - startedAt));
    } finally {
      setLoading(false);
    }
  }, [locale, platformAdmin, t]);

  useEffect(() => {
    void loadDashboardData();
  }, [loadDashboardData]);

  const handleRefresh = async () => {
    await refreshProfile();
    await loadDashboardData();
  };

  const branchCoverage = useMemo(() => {
    if (counts.employees === 0) return 0;
    return Math.round((employees.filter((employee) => !!employee.branchId).length / counts.employees) * 100);
  }, [counts.employees, employees]);

  const deviceCoverage = useMemo(() => {
    if (counts.employees === 0) return 0;
    return Math.round((counts.boundDevices / counts.employees) * 100);
  }, [counts.boundDevices, counts.employees]);

  const headerTitle = platformAdmin ? t('dashboard.title') : t('dashboard.tenantTitle');
  const headerDescription = platformAdmin ? t('dashboard.description') : t('dashboard.tenantDescription');

  return (
    <Flex direction="column" w="100%">
      <SharedPageHeader
        title={headerTitle}
        description={headerDescription}
        action={
          <Button leftIcon={<Icon as={MdRefresh as React.ElementType} />} variant="brand" onClick={handleRefresh} isLoading={authLoading || loading} w={{ base: '100%', md: 'auto' }}>
            {t('dashboard.refreshSession')}
          </Button>
        }
      />

      {loading ? (
        <Flex justify="center" align="center" h="300px" w="100%">
          <Spinner size="xl" thickness="4px" color="brand.500" speed="0.8s" />
        </Flex>
      ) : (
        <>
          <SimpleGrid columns={{ base: 1, sm: 2, xl: 4 }} gap="18px" mb="24px">
            {platformAdmin ? (
              <>
                <SharedStatCard label={t('dashboard.statsCompanies')} value={counts.companies} icon={<Icon as={MdBusiness as React.ElementType} w="22px" h="22px" />} tone="brand" />
                <SharedStatCard label={t('dashboard.statsSubscriptions')} value={counts.subscriptions} icon={<Icon as={MdSubscriptions as React.ElementType} w="22px" h="22px" />} tone="green" />
                <SharedStatCard label={t('dashboard.statsBranches')} value={counts.branches} icon={<Icon as={MdPlace as React.ElementType} w="22px" h="22px" />} tone="orange" />
                <SharedStatCard label={t('dashboard.statsAttendance')} value={counts.todayAttendance} icon={<Icon as={MdEventAvailable as React.ElementType} w="22px" h="22px" />} tone="purple" />
              </>
            ) : (
              <>
                <SharedStatCard label={t('dashboard.statsEmployees')} value={counts.employees} icon={<Icon as={MdGroups as React.ElementType} w="22px" h="22px" />} tone="brand" />
                <SharedStatCard label={t('dashboard.statsActiveEmployees')} value={counts.activeEmployees} icon={<Icon as={MdGroups as React.ElementType} w="22px" h="22px" />} tone="green" />
                <SharedStatCard label={t('dashboard.statsBranches')} value={counts.branches} icon={<Icon as={MdPlace as React.ElementType} w="22px" h="22px" />} tone="orange" />
                <SharedStatCard label={t('dashboard.statsAttendance')} value={counts.todayAttendance} icon={<Icon as={MdEventAvailable as React.ElementType} w="22px" h="22px" />} tone="purple" />
              </>
            )}
          </SimpleGrid>

          <Grid templateColumns={{ base: '1fr', xl: '2fr 1fr' }} gap="24px" w="100%">
            <GridItem>
              <Stack spacing="24px">
                <WeeklyTrendCard trend={weeklyTrend} />

                {!platformAdmin && (
                  <SharedCard p="24px">
                    <Heading size="sm" color={textColor} mb="18px">{t('dashboard.companyHealth')}</Heading>
                    <VStack spacing="18px" align="stretch">
                      <Box>
                        <Flex justify="space-between" mb="8px">
                          <Text fontSize="sm" fontWeight="800" color={textColor}>{t('dashboard.branchCoverage')}</Text>
                          <Text fontSize="sm" fontWeight="800" color={mutedText}>{branchCoverage}%</Text>
                        </Flex>
                        <Progress value={branchCoverage} colorScheme="brandScheme" borderRadius="8px" />
                      </Box>
                      <Box>
                        <Flex justify="space-between" mb="8px">
                          <Text fontSize="sm" fontWeight="800" color={textColor}>{t('dashboard.deviceCoverage')}</Text>
                          <Text fontSize="sm" fontWeight="800" color={mutedText}>{deviceCoverage}%</Text>
                        </Flex>
                        <Progress value={deviceCoverage} colorScheme="green" borderRadius="8px" />
                      </Box>
                    </VStack>
                  </SharedCard>
                )}
              </Stack>
            </GridItem>

            <GridItem>
              <Stack spacing="24px">
                <SharedCard p="24px">
                  <Flex align="center" gap="8px" mb="18px">
                    <Icon as={MdHistory as React.ElementType} color="brand.500" boxSize="20px" />
                    <Heading size="sm" color={textColor}>{platformAdmin ? t('dashboard.recentActivity') : t('dashboard.tenantRecentActivity')}</Heading>
                  </Flex>
                  <Stack spacing="0">
                    {activities.map((activity) => <ActivityRow key={activity.id} item={activity} />)}
                    {activities.length === 0 && <Text fontSize="sm" color={mutedText} py="20px" textAlign="center">{t('dashboard.noRecentActivity')}</Text>}
                  </Stack>
                </SharedCard>

                <SharedCard p="24px">
                  <Heading size="sm" color={textColor} mb="18px">{platformAdmin ? t('dashboard.systemStatus') : t('dashboard.workspaceStatus')}</Heading>
                  <VStack spacing="14px" align="stretch">
                    <Flex align="center" justify="space-between" p="12px 14px" borderRadius="8px" bg={surfaceBg}>
                      <Flex align="center" gap="10px">
                        <Icon as={MdCloudQueue as React.ElementType} color={isOnline ? 'green.500' : 'red.500'} boxSize="18px" />
                        <Text fontSize="sm" fontWeight="800" color={textColor}>{t('dashboard.apiStatus')}</Text>
                      </Flex>
                      <Badge colorScheme={isOnline ? 'green' : 'red'} borderRadius="full">{isOnline ? t('dashboard.online') : t('dashboard.offline')}</Badge>
                    </Flex>
                    <Flex align="center" justify="space-between" p="12px 14px" borderRadius="8px" bg={surfaceBg}>
                      <Flex align="center" gap="10px">
                        <Icon as={MdOutlinePhonelinkLock as React.ElementType} color="brand.500" boxSize="18px" />
                        <Text fontSize="sm" fontWeight="800" color={textColor}>{t('dashboard.boundDevices')}</Text>
                      </Flex>
                      <Text fontSize="sm" fontWeight="800" color={textColor}>{formatNumber(counts.boundDevices, locale)}</Text>
                    </Flex>
                    <Flex align="center" justify="space-between" p="12px 14px" borderRadius="8px" bg={surfaceBg}>
                      <Text fontSize="sm" fontWeight="800" color={textColor}>{t('dashboard.apiLatency')}</Text>
                      <Text fontSize="sm" fontWeight="800" color={textColor}>{latency} ms</Text>
                    </Flex>
                  </VStack>
                </SharedCard>

                {platformAdmin && subscriptions.length > 0 && (
                  <SharedCard p="24px">
                    <Heading size="sm" color={textColor} mb="14px">{t('dashboard.subscriptionSnapshot')}</Heading>
                    <Text fontSize="sm" color={mutedText}>{t('dashboard.activeSubscriptionsSummary', { count: counts.subscriptions })}</Text>
                  </SharedCard>
                )}
              </Stack>
            </GridItem>
          </Grid>
        </>
      )}
    </Flex>
  );
}