import React, { useMemo, useState } from 'react';
import { Box, Button, Flex, HStack, Icon, IconButton, Input, Spinner, Text, Tooltip, VStack, useColorModeValue, useToast } from '@chakra-ui/react';
import { Circle, MapContainer, Marker, Pane, TileLayer, useMap, useMapEvents } from 'react-leaflet';
import L, { LatLngExpression } from 'leaflet';
import { useTranslation } from 'react-i18next';
import { MdMap, MdMyLocation, MdSatelliteAlt } from 'react-icons/md';

type LocationChange = {
  latitude: number;
  longitude: number;
  address?: string;
};

type NominatimResult = {
  place_id: number;
  display_name: string;
  lat: string;
  lon: string;
};

type BranchLocationPickerProps = {
  latitude?: number;
  longitude?: number;
  radius?: number;
  address?: string;
  onChange: (location: LocationChange) => void;
};

type MapLayer = 'street' | 'satellite';
type TileOverlay = { attribution: string; url: string; opacity?: number };

const DEFAULT_CENTER: LatLngExpression = [30.0444, 31.2357];

const MAP_LAYERS: Record<MapLayer, { attribution: string; url: string; overlayLayers?: TileOverlay[] }> = {
  street: {
    attribution: '&copy; OSM',
    url: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
  },
  satellite: {
    attribution: '&copy; Esri, Maxar',
    url: 'https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}',
    overlayLayers: [
      {
        attribution: '&copy; Esri',
        url: 'https://server.arcgisonline.com/ArcGIS/rest/services/Reference/World_Boundaries_and_Places/MapServer/tile/{z}/{y}/{x}',
        opacity: 1,
      },
      {
        attribution: '&copy; Esri',
        url: 'https://server.arcgisonline.com/ArcGIS/rest/services/Reference/World_Transportation/MapServer/tile/{z}/{y}/{x}',
        opacity: 0.95,
      },
      {
        attribution: '&copy; CARTO, OSM',
        url: 'https://a.basemaps.cartocdn.com/light_only_labels/{z}/{x}/{y}.png',
        opacity: 0.9,
      },
    ],
  },
};

const markerIcon = L.divIcon({
  className: 'branch-location-marker',
  html: '<span></span>',
  iconSize: [22, 22],
  iconAnchor: [11, 11],
});

function MapClickHandler({ onChange }: { onChange: (location: LocationChange) => void }) {
  useMapEvents({
    click(event) {
      onChange({ latitude: event.latlng.lat, longitude: event.latlng.lng });
    },
  });
  return null;
}

function MapFlyTo({ center }: { center?: LatLngExpression }) {
  const map = useMap();

  React.useEffect(() => {
    if (center) {
      map.flyTo(center, Math.max(map.getZoom(), 15), { duration: 0.45 });
    }
  }, [center, map]);

  return null;
}

export default function BranchLocationPicker({ latitude, longitude, radius = 100, onChange }: BranchLocationPickerProps) {
  const { t, i18n } = useTranslation();
  const toast = useToast();
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<NominatimResult[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [isLocating, setIsLocating] = useState(false);
  const [mapLayer, setMapLayer] = useState<MapLayer>('street');
  const borderColor = useColorModeValue('gray.200', 'whiteAlpha.200');
  const panelBg = useColorModeValue('white', 'navy.900');
  const mutedColor = useColorModeValue('secondaryGray.600', 'secondaryGray.400');
  const layerButtonBg = useColorModeValue('white', 'navy.900');
  const isArabic = i18n.language?.startsWith('ar');
  const activeLayer = MAP_LAYERS[mapLayer];
  const activeOverlayLayers = useMemo<TileOverlay[]>(() => {
    if (mapLayer !== 'satellite') return [];
    if (isArabic) {
      return [
        {
          attribution: '&copy; OSM',
          url: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
          opacity: 0.62,
        },
      ];
    }
    return activeLayer.overlayLayers ?? [];
  }, [activeLayer.overlayLayers, isArabic, mapLayer]);

  const hasLocation = typeof latitude === 'number' && typeof longitude === 'number' && !(latitude === 0 && longitude === 0);
  const selectedCenter = useMemo<LatLngExpression | undefined>(() => {
    return hasLocation ? [latitude as number, longitude as number] : undefined;
  }, [hasLocation, latitude, longitude]);

  const layerToggleLabel = mapLayer === 'street' ? t('branches.switchToSatellite') : t('branches.switchToMap');

  const handleSearch = async () => {
    const cleanQuery = query.trim();
    if (!cleanQuery) return;

    setIsSearching(true);
    try {
      const params = new URLSearchParams({
        q: cleanQuery,
        format: 'json',
        limit: '5',
        addressdetails: '1',
        'accept-language': i18n.language,
      });
      const response = await fetch(`https://nominatim.openstreetmap.org/search?${params.toString()}`);
      if (!response.ok) throw new Error('Search failed');
      const data = (await response.json()) as NominatimResult[];
      setResults(data);
    } catch {
      toast({ title: t('branches.locationSearchError'), status: 'error', duration: 2500, isClosable: true });
    } finally {
      setIsSearching(false);
    }
  };

  const selectResult = (result: NominatimResult) => {
    const nextLatitude = Number(result.lat);
    const nextLongitude = Number(result.lon);
    onChange({ latitude: nextLatitude, longitude: nextLongitude, address: result.display_name });
    setQuery(result.display_name);
    setResults([]);
  };

  const reverseGeocode = async (nextLatitude: number, nextLongitude: number) => {
    try {
      const params = new URLSearchParams({
        lat: nextLatitude.toString(),
        lon: nextLongitude.toString(),
        format: 'json',
        zoom: '18',
        addressdetails: '1',
        'accept-language': i18n.language,
      });
      const response = await fetch(`https://nominatim.openstreetmap.org/reverse?${params.toString()}`);
      if (!response.ok) return undefined;
      const data = (await response.json()) as { display_name?: string };
      return data.display_name;
    } catch {
      return undefined;
    }
  };

  const updateLocationWithAddress = async (nextLatitude: number, nextLongitude: number) => {
    onChange({ latitude: nextLatitude, longitude: nextLongitude });
    const resolvedAddress = await reverseGeocode(nextLatitude, nextLongitude);
    if (resolvedAddress) {
      setQuery(resolvedAddress);
      onChange({ latitude: nextLatitude, longitude: nextLongitude, address: resolvedAddress });
    }
  };

  const handleUseCurrentLocation = () => {
    if (!navigator.geolocation) {
      toast({ title: t('branches.locationUnsupported'), status: 'warning', duration: 2500, isClosable: true });
      return;
    }

    setIsLocating(true);
    navigator.geolocation.getCurrentPosition(
      (position) => {
        onChange({ latitude: position.coords.latitude, longitude: position.coords.longitude });
        setIsLocating(false);
      },
      () => {
        toast({ title: t('branches.locationPermissionError'), status: 'error', duration: 3000, isClosable: true });
        setIsLocating(false);
      },
      { enableHighAccuracy: true, timeout: 12000, maximumAge: 30000 },
    );
  };

  return (
    <VStack align="stretch" spacing="10px">
      <HStack spacing="8px" align="stretch">
        <Input
          value={query}
          onChange={(event) => setQuery(event.target.value)}
          onKeyDown={(event) => {
            if (event.key === 'Enter') {
              event.preventDefault();
              void handleSearch();
            }
          }}
          placeholder={t('branches.locationSearchPlaceholder')}
          borderRadius="8px"
          fontSize="sm"
          h="42px"
        />
        <Button variant="outline" borderRadius="8px" onClick={handleSearch} isLoading={isSearching} minW="96px">
          {t('branches.locationSearch')}
        </Button>
      </HStack>

      {isSearching && (
        <Flex align="center" gap="8px" color={mutedColor} fontSize="sm">
          <Spinner size="sm" />
          <Text>{t('common.loading')}</Text>
        </Flex>
      )}

      {results.length > 0 && (
        <VStack align="stretch" spacing="6px" border="1px solid" borderColor={borderColor} borderRadius="8px" p="8px" bg={panelBg}>
          {results.map((result) => (
            <Button
              key={result.place_id}
              variant="ghost"
              justifyContent="flex-start"
              whiteSpace="normal"
              h="auto"
              minH="38px"
              py="8px"
              textAlign="start"
              fontSize="sm"
              onClick={() => selectResult(result)}
            >
              {result.display_name}
            </Button>
          ))}
        </VStack>
      )}

      <Box border="1px solid" borderColor={borderColor} borderRadius="8px" overflow="hidden" h="280px" position="relative">
        <MapContainer center={selectedCenter || DEFAULT_CENTER} zoom={hasLocation ? 15 : 11} style={{ height: '100%', width: '100%' }} scrollWheelZoom>
          <TileLayer key={`${mapLayer}-base`} attribution={activeLayer.attribution} url={activeLayer.url} />
          {activeOverlayLayers.length > 0 && (
            <Pane name={`branch-location-labels-${mapLayer}-${i18n.language}`} style={{ zIndex: 550, pointerEvents: 'none' }}>
              {activeOverlayLayers.map((layer, index) => (
                <TileLayer
                  key={`${mapLayer}-overlay-${index}`}
                  attribution={layer.attribution}
                  url={layer.url}
                  opacity={layer.opacity ?? 1}
                />
              ))}
            </Pane>
          )}
          <MapClickHandler onChange={(location) => { void updateLocationWithAddress(location.latitude, location.longitude); }} />
          <MapFlyTo center={selectedCenter} />
          {selectedCenter && (
            <>
              <Marker position={selectedCenter} icon={markerIcon} />
              <Circle center={selectedCenter} radius={Number(radius) || 100} pathOptions={{ color: '#2563eb', fillColor: '#2563eb', fillOpacity: 0.12 }} />
            </>
          )}
        </MapContainer>
        <VStack position="absolute" top="12px" right="12px" zIndex={600} spacing="8px">
          <Tooltip label={t('branches.useCurrentLocation')} placement="start">
            <IconButton
              aria-label={t('branches.useCurrentLocation')}
              icon={<Icon as={MdMyLocation as React.ElementType} boxSize="18px" />}
              size="sm"
              borderRadius="8px"
              bg={layerButtonBg}
              boxShadow="0 10px 24px rgba(15, 23, 42, 0.18)"
              isLoading={isLocating}
              onClick={handleUseCurrentLocation}
            />
          </Tooltip>
          <Tooltip label={layerToggleLabel} placement="start">
            <IconButton
              aria-label={layerToggleLabel}
              icon={<Icon as={(mapLayer === 'street' ? MdSatelliteAlt : MdMap) as React.ElementType} boxSize="18px" />}
              size="sm"
              borderRadius="8px"
              bg={layerButtonBg}
              boxShadow="0 10px 24px rgba(15, 23, 42, 0.18)"
              onClick={() => setMapLayer((current) => (current === 'street' ? 'satellite' : 'street'))}
            />
          </Tooltip>
        </VStack>
      </Box>

      <Text fontSize="xs" color={mutedColor}>
        {t('branches.locationHint')}
      </Text>
    </VStack>
  );
}





