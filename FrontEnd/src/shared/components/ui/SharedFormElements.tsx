import React from 'react';
import {
  Button,
  ButtonProps,
  Checkbox,
  CheckboxProps,
  FormControl,
  FormErrorMessage,
  FormLabel,
  IconButton,
  IconButtonProps,
  Input,
  InputProps,
  Select,
  SelectProps,
  Switch,
  SwitchProps,
  Text,
  Textarea,
  TextareaProps,
  useColorModeValue,
} from '@chakra-ui/react';

interface FormFieldWrapperProps {
  label?: string;
  error?: string;
  isRequired?: boolean;
}

function useFieldColors() {
  return {
    labelColor: useColorModeValue('secondaryGray.900', 'white'),
    controlBg: useColorModeValue('white', 'navy.900'),
    borderColor: useColorModeValue('gray.200', 'whiteAlpha.100'),
  };
}

function errorDescriptionId(name?: string, error?: string) {
  return name && error ? `${name}-error` : undefined;
}

export function SharedButton(props: ButtonProps) {
  return <Button borderRadius="8px" fontWeight="700" {...props} />;
}

export function SharedIconButton(props: IconButtonProps) {
  return <IconButton borderRadius="8px" {...props} />;
}

export interface SharedInputProps extends InputProps, FormFieldWrapperProps {}

export const SharedInput = React.forwardRef<HTMLInputElement, SharedInputProps>(
  ({ label, error, isRequired, name, ...props }, ref) => {
    const { labelColor, controlBg, borderColor } = useFieldColors();
    const describedBy = errorDescriptionId(name, error);

    return (
      <FormControl isInvalid={!!error} isRequired={isRequired} mb="16px">
        {label && (
          <FormLabel ms="2px" fontSize="sm" fontWeight="700" color={labelColor} mb="7px">
            {label}
          </FormLabel>
        )}
        <Input
          ref={ref}
          name={name}
          aria-invalid={!!error}
          aria-describedby={describedBy}
          bg={controlBg}
          borderColor={borderColor}
          borderRadius="8px"
          fontSize="sm"
          h="42px"
          _focus={{ borderColor: 'brand.500', boxShadow: '0 0 0 1px var(--chakra-colors-brand-500)' }}
          {...props}
        />
        {error && <FormErrorMessage id={describedBy} ms="2px">{error}</FormErrorMessage>}
      </FormControl>
    );
  }
);

SharedInput.displayName = 'SharedInput';

export interface SharedTextareaProps extends TextareaProps, FormFieldWrapperProps {}

export const SharedTextarea = React.forwardRef<HTMLTextAreaElement, SharedTextareaProps>(
  ({ label, error, isRequired, name, ...props }, ref) => {
    const { labelColor, controlBg, borderColor } = useFieldColors();
    const describedBy = errorDescriptionId(name, error);

    return (
      <FormControl isInvalid={!!error} isRequired={isRequired} mb="16px">
        {label && (
          <FormLabel ms="2px" fontSize="sm" fontWeight="700" color={labelColor} mb="7px">
            {label}
          </FormLabel>
        )}
        <Textarea
          ref={ref}
          name={name}
          aria-invalid={!!error}
          aria-describedby={describedBy}
          bg={controlBg}
          borderColor={borderColor}
          borderRadius="8px"
          fontSize="sm"
          minH="96px"
          resize="vertical"
          _focus={{ borderColor: 'brand.500', boxShadow: '0 0 0 1px var(--chakra-colors-brand-500)' }}
          {...props}
        />
        {error && <FormErrorMessage id={describedBy} ms="2px">{error}</FormErrorMessage>}
      </FormControl>
    );
  }
);

SharedTextarea.displayName = 'SharedTextarea';

export interface SharedSelectProps extends SelectProps, FormFieldWrapperProps {
  options: Array<{ value: string | number; label: string }>;
  placeholderLabel?: string;
}

export const SharedSelect = React.forwardRef<HTMLSelectElement, SharedSelectProps>(
  ({ label, error, isRequired, options, placeholderLabel, name, ...props }, ref) => {
    const { labelColor, controlBg, borderColor } = useFieldColors();
    const describedBy = errorDescriptionId(name, error);

    return (
      <FormControl isInvalid={!!error} isRequired={isRequired} mb="16px">
        {label && (
          <FormLabel ms="2px" fontSize="sm" fontWeight="700" color={labelColor} mb="7px">
            {label}
          </FormLabel>
        )}
        <Select
          ref={ref}
          name={name}
          aria-invalid={!!error}
          aria-describedby={describedBy}
          h="42px"
          fontSize="sm"
          borderRadius="8px"
          bg={controlBg}
          borderColor={borderColor}
          _focus={{ borderColor: 'brand.500', boxShadow: '0 0 0 1px var(--chakra-colors-brand-500)' }}
          placeholder={placeholderLabel}
          {...props}
        >
          {options.map((opt) => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </Select>
        {error && <FormErrorMessage id={describedBy} ms="2px">{error}</FormErrorMessage>}
      </FormControl>
    );
  }
);

SharedSelect.displayName = 'SharedSelect';

export interface SharedCheckboxProps extends CheckboxProps {
  label: string;
  error?: string;
}

export const SharedCheckbox = React.forwardRef<HTMLInputElement, SharedCheckboxProps>(
  ({ label, error, name, ...props }, ref) => {
    const { labelColor } = useFieldColors();
    const describedBy = errorDescriptionId(name, error);

    return (
      <FormControl isInvalid={!!error} mb="16px">
        <Checkbox ref={ref} name={name} aria-invalid={error ? true : undefined} aria-describedby={describedBy} colorScheme="brandScheme" borderRadius="4px" {...props}>
          <Text fontSize="sm" fontWeight="600" color={labelColor}>
            {label}
          </Text>
        </Checkbox>
        {error && <FormErrorMessage id={describedBy} ms="2px">{error}</FormErrorMessage>}
      </FormControl>
    );
  }
);

SharedCheckbox.displayName = 'SharedCheckbox';

export interface SharedSwitchProps extends SwitchProps {
  label: string;
  error?: string;
}

export const SharedSwitch = React.forwardRef<HTMLInputElement, SharedSwitchProps>(
  ({ label, error, name, ...props }, ref) => {
    const { labelColor } = useFieldColors();
    const describedBy = errorDescriptionId(name, error);

    return (
      <FormControl isInvalid={!!error} display="flex" alignItems="center" justifyContent="space-between" mb="16px">
        <FormLabel mb="0" color={labelColor} fontSize="sm" fontWeight="700">
          {label}
        </FormLabel>
        <Switch ref={ref} name={name} aria-invalid={error ? true : undefined} aria-describedby={describedBy} colorScheme="brandScheme" {...props} />
        {error && <FormErrorMessage id={describedBy} ms="2px">{error}</FormErrorMessage>}
      </FormControl>
    );
  }
);

SharedSwitch.displayName = 'SharedSwitch';

