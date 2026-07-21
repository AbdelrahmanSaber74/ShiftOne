import React from 'react';
import {
  Box,
  Button,
  Checkbox,
  FormControl,
  FormLabel,
  Grid,
  Input,
  Select,
  useColorModeValue,
} from '@chakra-ui/react';
import { useTranslation } from 'react-i18next';

export type FieldKind = 'text' | 'number' | 'checkbox' | 'password' | 'select';

export interface FieldConfig {
  key: string;
  label: string;
  kind?: FieldKind;
  options?: Array<{ label: string; value: string }>;
}

interface QuickFormProps {
  fields: FieldConfig[];
  values: Record<string, any>;
  setValues: React.Dispatch<React.SetStateAction<Record<string, any>>>;
  onSubmit: () => Promise<void>;
  loading: boolean;
  submitLabel?: string;
}

export default function QuickForm({
  fields,
  values,
  setValues,
  onSubmit,
  loading,
  submitLabel,
}: QuickFormProps) {
  const { t } = useTranslation();
  const bg = useColorModeValue('white', 'navy.800');

  const handleFieldChange = (key: string, value: any) => {
    setValues((current) => ({
      ...current,
      [key]: value,
    }));
  };

  return (
    <Box bg={bg} borderRadius="8px" p="20px" mb="20px" boxShadow="sm">
      <Grid templateColumns={{ base: '1fr', md: 'repeat(4, 1fr)' }} gap="14px" alignItems="end">
        {fields.map((field) => (
          <FormControl key={field.key}>
            {field.kind !== 'checkbox' && (
              <FormLabel fontSize="sm" mb="8px">
                {field.label}
              </FormLabel>
            )}
            {field.kind === 'checkbox' ? (
              <Checkbox
                isChecked={Boolean(values[field.key])}
                onChange={(event) => handleFieldChange(field.key, event.target.checked)}
                py="10px"
              >
                {field.label}
              </Checkbox>
            ) : field.kind === 'select' ? (
              <Select
                value={values[field.key] ?? ''}
                onChange={(event) => handleFieldChange(field.key, event.target.value)}
              >
                <option value="">{t('common.select')}</option>
                {(field.options || []).map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </Select>
            ) : (
              <Input
                type={
                  field.kind === 'number'
                    ? 'number'
                    : field.kind === 'password'
                    ? 'password'
                    : 'text'
                }
                value={values[field.key] ?? ''}
                onChange={(event) =>
                  handleFieldChange(
                    field.key,
                    field.kind === 'number'
                      ? event.target.value === ''
                        ? ''
                        : Number(event.target.value)
                      : event.target.value
                  )
                }
              />
            )}
          </FormControl>
        ))}
        <Button variant="brand" onClick={onSubmit} isLoading={loading}>
          {submitLabel ?? t('common.save')}
        </Button>
      </Grid>
    </Box>
  );
}
