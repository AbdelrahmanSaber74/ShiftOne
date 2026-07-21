export const formatNumber = (num: number, locale = 'en-US'): string => {
  return new Intl.NumberFormat(locale).format(num);
};

export const formatCurrency = (amount: number, currency = 'SAR', locale = 'en-US'): string => {
  return new Intl.NumberFormat(locale, { style: 'currency', currency }).format(amount);
};

export const formatDate = (date: Date | string, locale = 'en-US', options?: Intl.DateTimeFormatOptions): string => {
  const d = typeof date === 'string' ? new Date(date) : date;
  return new Intl.DateTimeFormat(locale, options || { dateStyle: 'medium' }).format(d);
};
