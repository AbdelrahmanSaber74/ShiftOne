import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from 'shared/contexts/AuthContext';
import { Alert, AlertIcon, Box, Button, Flex, FormControl, FormErrorMessage, FormLabel, Heading, Icon, Input, InputGroup, InputRightElement, Text, useColorModeValue } from '@chakra-ui/react';
import DefaultAuth from 'shared/layouts/auth/Default';
import illustration from 'shared/assets/img/auth/auth.png';
import { MdOutlineRemoveRedEye as MdEye } from 'react-icons/md';
import { RiEyeCloseLine } from 'react-icons/ri';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { signInSchema, SignInFormValues } from '../validations/signInSchema';
import { translateServerMessage } from 'shared/utils/errorUtils';

export default function SignInPage() {
  const textColor = useColorModeValue('navy.700', 'white');
  const textColorSecondary = 'gray.400';
  const brandStars = useColorModeValue('brand.500', 'brand.400');
  const { t } = useTranslation();
  const schema = useMemo(() => signInSchema(t), [t]);

  const [show, setShow] = useState(false);
  const [error, setError] = useState('');

  const { login } = useAuth();
  const navigate = useNavigate();
  const { register, handleSubmit, formState: { errors, isSubmitting }, clearErrors } = useForm<SignInFormValues>({
    resolver: zodResolver(schema) as any,
    mode: 'onSubmit',
    reValidateMode: 'onChange',
    defaultValues: { emailOrPhone: '', password: '' },
  });

  const onSubmit = handleSubmit(async (values) => {
    setError('');
    try {
      const result = await login(values.emailOrPhone, values.password);
      if (result.success) {
        navigate('/admin/default');
      } else {
        setError(translateServerMessage({ response: { data: { message: result.message } } }, t('auth.loginFailed')));
      }
    } catch (err) {
      setError(translateServerMessage(err, t('common.error')));
    }
  });

  return (
    <DefaultAuth illustrationBackground={illustration}>
      <Flex direction="column" w="100%" maxW="420px" mx="auto" justifyContent="center" alignItems="stretch">
        <Box mb="36px">
          <Heading color={textColor} fontSize="36px" mb="10px">{t('auth.signIn')}</Heading>
          <Text ms="4px" color={textColorSecondary} fontWeight="400" fontSize="md">{t('auth.enterCredentials')}</Text>
        </Box>
        <Flex zIndex="2" direction="column" w="100%" background="transparent" borderRadius="8px">
          {error ? (
            <Alert status="error" borderRadius="8px" mb="24px">
              <AlertIcon />
              <Text fontSize="sm">{error}</Text>
            </Alert>
          ) : null}

          <form onSubmit={(e) => e.preventDefault()} noValidate>
            <FormControl isInvalid={!!errors.emailOrPhone} mb="24px">
              <FormLabel display="flex" ms="4px" fontSize="sm" fontWeight="500" color={textColor} mb="8px">
                {t('auth.emailOrPhone')}<Text color={brandStars}>*</Text>
              </FormLabel>
              <Input
                variant="auth"
                fontSize="sm"
                type="text"
                placeholder={t('auth.emailOrPhonePlaceholder')}
                fontWeight="500"
                size="lg"
                {...register('emailOrPhone', { onChange: () => { clearErrors('emailOrPhone'); setError(''); } })}
              />
              <FormErrorMessage>{errors.emailOrPhone?.message}</FormErrorMessage>
            </FormControl>
            <FormControl isInvalid={!!errors.password} mb="24px">
              <FormLabel ms="4px" fontSize="sm" fontWeight="500" color={textColor} display="flex">
                {t('auth.password')}<Text color={brandStars}>*</Text>
              </FormLabel>
              <InputGroup size="md">
                <Input
                  fontSize="sm"
                  placeholder={t('auth.passwordPlaceholder')}
                  size="lg"
                  type={show ? 'text' : 'password'}
                  variant="auth"
                  {...register('password', { onChange: () => { clearErrors('password'); setError(''); } })}
                />
                <InputRightElement display="flex" alignItems="center" mt="4px">
                  <Icon color={textColorSecondary} _hover={{ cursor: 'pointer' }} as={(show ? RiEyeCloseLine : MdEye) as React.ElementType} onClick={() => setShow((value) => !value)} />
                </InputRightElement>
              </InputGroup>
              <FormErrorMessage>{errors.password?.message}</FormErrorMessage>
            </FormControl>
            <Button fontSize="sm" variant="brand" fontWeight="500" w="100%" h="50px" mt="10px" mb="24px" type="button" onClick={() => void onSubmit()} isLoading={isSubmitting} loadingText={t('auth.signingIn')}>
              {t('auth.signIn')}
            </Button>
          </form>
        </Flex>
      </Flex>
    </DefaultAuth>
  );
}


