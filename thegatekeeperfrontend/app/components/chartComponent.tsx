"use client"
import { closeChartView } from '@/store/features/baseComponentsSlice';
import { lightTheme, darkTheme, vibrantTheme } from "@/context/themes";
import { useAppSelector, useAppDispatch } from '@/store/hooks';
import { useState, useEffect } from 'react';
import { Line } from 'react-chartjs-2';
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip as ChartJsTooltip,
    Legend
} from 'chart.js';

// Register Chart.js components
ChartJS.register(
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    ChartJsTooltip,
    Legend
);
import { Typography } from '@mui/material';



export const ChartComponent: React.FC = () => {
    const theme = useAppSelector(state => state.userSlice.selectedTheme);
    const visible = useAppSelector(state => state.viewStateSlice.chartView.visible);
    const userName = useAppSelector(state => state.viewStateSlice.chartView.userName);
    const dispatch = useAppDispatch();
    const [chartData, setChartData] = useState<any[]>([]);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        setLoading(true);
        (async () => {
            try {
                if (!userName) {
                    setLoading(false);
                    return;
                }
                const response = await fetch(`/api/TheGateKeeper/getHistory?userName=${encodeURIComponent(userName)}`);
                if (!response.ok) {
                    throw new Error('Failed to fetch user history');
                }
                const data = await response.json();
                setChartData(
                    data?.rankTimeLine?.map((x: any) => ({
                        date: new Date(x.dateTime).toLocaleDateString(),
                        value: x.leaguePoints
                    })) || []
                );
            } catch (err: any) {
                console.error(err.message || 'Failed to fetch user history');
            } finally {
                setLoading(false);
            }
        })();
    }, [userName]);

    const handleClose = () => {
        dispatch(closeChartView());
    };

    if (!visible) return null;

    return (
        <div
            onPointerDown={handleClose}
            style={{
                position: 'fixed',
                top: 0,
                left: 0,
                width: '100vw',
                height: '100vh',
                background: 'rgba(0,0,0,0.5)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                zIndex: 1000
            }}
        >
            <div
                onPointerDown={e => e.stopPropagation()}
                style={{
                    background: theme === 'light' ? lightTheme.palette.background.paper :
                        theme === 'dark' ? darkTheme.palette.background.paper :
                            vibrantTheme.palette.background.paper,
                    borderRadius: '12px',
                    boxShadow: '0 2px 16px rgba(0,0,0,0.2)',
                    padding: '32px 24px 24px 24px',
                    minWidth: '650px',
                    position: 'relative',
                    maxWidth: '90vw',
                    maxHeight: '90vh',
                    overflow: 'auto'
                }}>
                <button
                    onPointerDown={handleClose}
                    style={{
                        position: 'absolute',
                        top: 12,
                        right: 12,
                        background: 'transparent',
                        border: 'none',
                        fontSize: 24,
                        cursor: 'pointer',
                        color: '#888'
                    }}
                    aria-label="Close"
                >
                    &times;
                </button>
                {loading ? (
                    <div style={{ textAlign: 'center', padding: '40px 0' }}>Loading...</div>
                ) : (
                    <div style={{ display: 'flex', alignItems: 'center' }}>
                        <Line
                            data={{
                                labels: chartData.map((d: any) => d.date),
                                datasets: [
                                    {
                                        label: 'League Points',
                                        data: chartData.map((d: any) => d.value),
                                        borderColor: '#8884d8',
                                        backgroundColor: 'rgba(136,132,216,0.2)',
                                        tension: 0.4,
                                    },
                                ],
                            }}
                            options={{
                                responsive: true,
                                plugins: {
                                    legend: { display: true, position: 'top' },
                                    title: { display: false },
                                    tooltip: { enabled: true },
                                },
                                scales: {
                                    y: {
                                        min: 0,
                                        max: 100,
                                        title: { display: true, text: 'League Points' },
                                    },
                                    x: {
                                        title: { display: true, text: 'Date' },
                                    },
                                },
                            }}
                        />
                        <div style={{ marginLeft: 16, fontWeight: 'bold' }}>
                            <Typography component="span" color="primary" >{"Select Theme"}</Typography>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}