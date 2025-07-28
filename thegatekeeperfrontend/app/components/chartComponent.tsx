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
import annotationPlugin from 'chartjs-plugin-annotation';

// Register Chart.js components
ChartJS.register(
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    ChartJsTooltip,
    Legend,
    annotationPlugin
);
import { Typography } from '@mui/material';
import { getTierRankLeaguePointsFromTotal } from '../common/tierCalculator';
import { domainUrlPrefix } from '@/store/backEndCalls';



export const ChartComponent: React.FC = () => {
    const theme = useAppSelector(state => state.userSlice.selectedTheme);
    const visible = useAppSelector(state => state.viewStateSlice.chartView.visible);
    const userName = useAppSelector(state => state.viewStateSlice.chartView.userName);
    const dispatch = useAppDispatch();
    const [chartData, setChartData] = useState<any[]>([]);
    const [loading, setLoading] = useState(false);

    const lpToTier = (lp: any) => {
        switch (true) {
            case lp >= 2700:
                return 'Diamond 1';
            case lp >= 2600:
                return 'Diamond 2';
            case lp >= 2500:
                return 'Diamond 3';
            case lp >= 2400:
                return 'Diamond 4';
            case lp >= 2300:
                return 'Emerald 1';
            case lp >= 2200:
                return 'Emerald 2';
            case lp >= 2100:
                return 'Emerald 3';
            case lp >= 2000:
                return 'Emerald 4';
            case lp >= 1900:
                return 'Platinum 1';
            case lp >= 1800:
                return 'Platinum 2';
            case lp >= 1700:
                return 'Platinum 3';
            case lp >= 1600:
                return 'Platinum 4';
            case lp >= 1500:
                return 'Gold 1';
            case lp >= 1400:
                return 'Gold 2';
            case lp >= 1300:
                return 'Gold 3';
            case lp >= 1200:
                return 'Gold 4';
            case lp >= 1100:
                return 'Silver 1';
            case lp >= 1000:
                return 'Silver 2';
            case lp >= 900:
                return 'Silver 3';
            case lp >= 800:
                return 'Silver 4';
            case lp >= 700:
                return 'Bronze 1';
            case lp >= 600:
                return 'Bronze 2';
            case lp >= 500:
                return 'Bronze 3';
            case lp >= 400:
                return 'Bronze 4';
            case lp >= 300:
                return 'Iron 1';
            case lp >= 200:
                return 'Iron 2';
            case lp >= 100:
                return 'Iron 3';
            default:
                return `Iron 4`;
        }
    };

    useEffect(() => {
        setLoading(true);
        (async () => {
            try {
                if (!userName) {
                    setLoading(false);
                    return;
                }
                const response = await fetch(`${domainUrlPrefix()}/api/TheGateKeeper/getHistory?userName=${encodeURIComponent(userName)}`);
                if (!response.ok) {
                    throw new Error('Failed to fetch user history');
                }
                const data = await response.json();
                setChartData(
                    data?.rankTimeLine?.map((x: any) => ({
                        date: new Date(x.dateTime).toLocaleDateString(),
                        leaguePoints: x.combinedPoints,
                        rank: getTierRankLeaguePointsFromTotal(x.combinedPoints).rank,
                        tier: getTierRankLeaguePointsFromTotal(x.combinedPoints).tier
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
                    <Line
                        data={{
                            labels: chartData.map((d) => d.date),
                            datasets: [
                                {
                                    label: userName,
                                    data: chartData.map((d) => d.leaguePoints),
                                    borderColor: '#00bcd4',
                                    backgroundColor: 'rgba(0,188,212,0.2)',
                                    tension: 0.4,
                                },
                            ],
                        }}
                        options={{
                            responsive: true,
                            plugins: {
                                legend: { display: true, position: 'top' },
                                tooltip: {
                                    enabled: true,
                                    callbacks: {
                                        label: function(context) {
                                            const index = context.dataIndex;
                                            const rank = context.chart.data.datasets[0].data[index];
                                            // Find the corresponding chartData entry
                                            const chartDataEntry = chartData[index];
                                            return `${lpToTier(chartDataEntry?.leaguePoints) || ''} (${chartDataEntry?.leaguePoints%100} LP)`;
                                        }
                                    }
                                },
                                annotation: {
                                    annotations: (() => {
                                        // Define tier boundaries and labels
                                        const boundaries = [
                                            { value: 100, label: 'Iron 3' },
                                            { value: 200, label: 'Iron 2' },
                                            { value: 300, label: 'Iron 1' },
                                            { value: 400, label: 'Bronze 4' },
                                            { value: 500, label: 'Bronze 3' },
                                            { value: 600, label: 'Bronze 2' },
                                            { value: 700, label: 'Bronze 1' },
                                            { value: 800, label: 'Silver 4' },
                                            { value: 900, label: 'Silver 3' },
                                            { value: 1000, label: 'Silver 2' },
                                            { value: 1100, label: 'Silver 1' },
                                            { value: 1200, label: 'Gold 4' },
                                            { value: 1300, label: 'Gold 3' },
                                            { value: 1400, label: 'Gold 2' },
                                            { value: 1500, label: 'Gold 1' },
                                            { value: 1600, label: 'Platinum 4' },
                                            { value: 1700, label: 'Platinum 3' },
                                            { value: 1800, label: 'Platinum 2' },
                                            { value: 1900, label: 'Platinum 1' },
                                            { value: 2000, label: 'Emerald 4' },
                                            { value: 2100, label: 'Emerald 3' },
                                            { value: 2200, label: 'Emerald 2' },
                                            { value: 2300, label: 'Emerald 1' },
                                            { value: 2400, label: 'Diamond 4' },
                                            { value: 2500, label: 'Diamond 3' },
                                            { value: 2600, label: 'Diamond 2' },
                                            { value: 2700, label: 'Diamond 1' },
                                        ];
                                        const annos: any = {};
                                        boundaries.forEach((b, i) => {
                                            annos[`tierBoundary${i}`] = {
                                                type: 'line',
                                                yMin: b.value,
                                                yMax: b.value,
                                                borderColor: 'rgba(200,200,200,0.7)',
                                                borderWidth: 2,
                                                borderDash: [6, 6]
                                            };
                                        });
                                        return annos;
                                    })(),
                                },
                            },
                            scales: {
                                y: {
                                    ticks: {
                                        stepSize: 100,
                                        callback: function (value) {
                                            return lpToTier(value)
                                        },
                                    },
                                    max: Math.max(...chartData.map((d) => d.leaguePoints))*1.05,
                                    min: Math.min(...chartData.map((d) => d.leaguePoints))*0.95,
                                    title: { display: true, text: 'Rank' },
                                    grid: { drawOnChartArea: true },
                                },
                                x: {
                                    title: { display: true, text: 'Date' },
                                },
                            },
                        }}
                    />
                )}
            </div>
        </div>
    );
}