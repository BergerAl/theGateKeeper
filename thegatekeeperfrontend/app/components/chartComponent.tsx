
import { closeChartView } from '@/store/features/baseComponentsSlice';
import { lightTheme, darkTheme, vibrantTheme } from "@/context/themes";
import { useAppSelector, useAppDispatch } from '@/store/hooks';
import { useState, useEffect } from 'react';
import { LineChart, Line, XAxis, YAxis, Tooltip, CartesianGrid } from 'recharts';


export const ChartComponent: React.FC = () => {
    const historyEntry = useAppSelector(state => state.viewStateSlice.chartView);
    const theme = useAppSelector(state => state.userSlice.selectedTheme);
    const dispatch = useAppDispatch();

    // Local state for chart data
    const [chartData, setChartData] = useState<any[]>([]);

    useEffect(() => {
        setChartData(
            historyEntry?.entry?.rankTimeLine?.map(x => ({
                date: new Date(x.dateTime).toLocaleDateString(),
                value: x.leaguePoints
            })) || []
        );
    }, [historyEntry]);

    // You may need to replace this with your actual action to hide the chart modal
    const handleClose = () => {
        dispatch(closeChartView());
    };

    if (!historyEntry.visible) return null;

    return (
        <div
            onClick={() => dispatch(closeChartView())}
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
            <div style={{
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
                    onClick={handleClose}
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
                <LineChart
                    width={600}
                    height={300}
                    data={chartData}
                >
                    <XAxis dataKey="date" />
                    <YAxis />
                    <Tooltip />
                    <CartesianGrid stroke="#eee" strokeDasharray="5 5" />
                    <Line type="monotone" dataKey="value" stroke="#8884d8" />
                </LineChart>
            </div>
        </div >
    );
}