import React, { useEffect, useState } from 'react';
import './App.css';
import { useAppSelector } from '@/store/hooks';
import Confetti from 'react-confetti';
import { Box, Typography } from '@mui/material';

export const ResultPage: React.FC = () => {
    const gateKeeperName = useAppSelector(state => state.viewStateSlice.gateKeeperInfo.name)
    const [showConfetti, setShowConfetti] = useState(true);

    // Stop the confetti after 5 seconds
    useEffect(() => {
        const timer = setTimeout(() => setShowConfetti(false), 50);
        return () => clearTimeout(timer);
    }, []);
    return (
        <Box>
            
            {!showConfetti && <Confetti />}
            <Typography color="primary" variant="h4" align="center" mt={4}>
                {`Gratulation to our new GateKeeper: ${gateKeeperName} 🎉`}
            </Typography>
        </Box>
    )
}
