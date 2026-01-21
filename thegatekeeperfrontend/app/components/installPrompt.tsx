import React, { useEffect, useState } from 'react';
import { Button, Box, Card, useTheme } from '@mui/material';
import { useAppSelector } from '@/store/hooks';

const PROMPT_STORAGE_KEY = 'gatekeeper-install-prompt-dismissed';
const PROMPT_RESHOW_DAYS = 7;

export const PWAInstallPrompt: React.FC = () => {
  const [deferredPrompt, setDeferredPrompt] = useState<any>(null);
  const [showPrompt, setShowPrompt] = useState(false);
  const theme = useTheme();
  const themeMode = useAppSelector(state => state.userSlice.selectedTheme);

  useEffect(() => {
    // Check on mount if prompt should be shown based on dismissal time
    const dismissedTime = localStorage.getItem(PROMPT_STORAGE_KEY);
    if (dismissedTime) {
      const dismissedDate = new Date(dismissedTime).getTime();
      const now = new Date().getTime();
      const daysPassed = (now - dismissedDate) / (1000 * 60 * 60 * 24);
      
      if (daysPassed < PROMPT_RESHOW_DAYS) {
        setShowPrompt(false);
        return;
      }
    }

    const handler = (e: Event) => {
      e.preventDefault();
      console.log('beforeinstallprompt event fired');
      setDeferredPrompt(e);
      setShowPrompt(true);
    };

    window.addEventListener('beforeinstallprompt', handler);

    return () => window.removeEventListener('beforeinstallprompt', handler);
  }, []);

  const handleInstall = async () => {
    if (deferredPrompt) {
      deferredPrompt.prompt();
      await deferredPrompt.userChoice;
      setDeferredPrompt(null);
      setShowPrompt(false);
      // Clear the dismissal timestamp on successful install
      localStorage.removeItem(PROMPT_STORAGE_KEY);
    }
  };

  const handleDismiss = () => {
    setShowPrompt(false);
    // Store the current timestamp when user clicks "Not now"
    localStorage.setItem(PROMPT_STORAGE_KEY, new Date().toISOString());
  };

  if (!showPrompt) return null;

  const isDark = themeMode === 'dark';
  const isVibrant = themeMode === 'vibrant';

  return (
    <Box sx={{ position: 'fixed', bottom: 20, right: 20, zIndex: 999 }}>
      <Card sx={{ 
        p: 3, 
        maxWidth: 320,
        backgroundColor: isDark ? '#2d2d2d' : isVibrant ? '#ffebee' : '#f0f8ff',
        border: `3px solid ${theme.palette.primary.main}`,
        borderRadius: '12px',
        boxShadow: `0 8px 24px ${isDark ? 'rgba(144, 202, 249, 0.2)' : 'rgba(0, 102, 204, 0.3)'}`,
      }}>
        <Box sx={{ mb: 2 }}>
          <strong style={{ fontSize: '18px', color: theme.palette.primary.main }}>📱 Install The GateKeeper?</strong>
          <p style={{ marginTop: '8px', marginBottom: 0, color: isDark ? '#e0e0e0' : '#333', fontSize: '14px' }}>
            Get quick access to your rankings on your home screen.
          </p>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button 
            variant="contained" 
            onClick={handleInstall} 
            fullWidth
            sx={{ backgroundColor: theme.palette.primary.main, '&:hover': { backgroundColor: theme.palette.primary.dark } }}
          >
            Install
          </Button>
          <Button 
            variant="outlined" 
            onClick={handleDismiss} 
            fullWidth
            sx={{ 
              borderColor: theme.palette.primary.main, 
              color: theme.palette.primary.main, 
              '&:hover': { backgroundColor: isDark ? 'rgba(144, 202, 249, 0.1)' : isVibrant ? 'rgba(255, 64, 129, 0.1)' : 'rgba(25, 118, 210, 0.1)' } 
            }}
          >
            Not now
          </Button>
        </Box>
      </Card>
    </Box>
  );
};