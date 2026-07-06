'use client'
import React, { useEffect, useState } from 'react'
import dynamic from 'next/dynamic'
import Dialog from '@mui/material/Dialog'
import DialogContent from '@mui/material/DialogContent'
import DialogTitle from '@mui/material/DialogTitle'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Typography from '@mui/material/Typography'
import CircularProgress from '@mui/material/CircularProgress'
import { domainUrlPrefix } from '@/store/backEndCalls'

const Wheel = dynamic(
    () => import('react-custom-roulette').then(m => m.Wheel),
    { ssr: false }
)

interface SpinWheelModalProps {
    open: boolean
    onClose: () => void
    username: string
    accessToken: string | undefined
}

const SEGMENT_COLORS = [
    '#E74C3C', '#3498DB', '#2ECC71', '#F39C12',
    '#9B59B6', '#1ABC9C', '#E67E22', '#2980B9',
    '#27AE60', '#D35400', '#8E44AD', '#16A085',
]

export const SpinWheelModal: React.FC<SpinWheelModalProps> = ({ open, onClose, username, accessToken }) => {
    const [wheelData, setWheelData] = useState<{ option: string; style: { backgroundColor: string; textColor: string } }[]>([])
    const [mustSpin, setMustSpin] = useState(false)
    const [prizeNumber, setPrizeNumber] = useState(0)
    const [result, setResult] = useState<string | null>(null)
    const [loading, setLoading] = useState(false)
    const [notifying, setNotifying] = useState(false)

    useEffect(() => {
        if (!open) return
        setResult(null)
        setMustSpin(false)
        setLoading(true)

        fetch(`${domainUrlPrefix()}/api/wheel/options`)
            .then(r => r.ok ? r.json() : Promise.reject('Failed to fetch options'))
            .then((data: { options: string[] }) => {
                const options = data.options ?? []
                setWheelData(
                    options.map((opt, i) => ({
                        option: opt,
                        style: {
                            backgroundColor: SEGMENT_COLORS[i % SEGMENT_COLORS.length],
                            textColor: '#ffffff',
                        },
                    }))
                )
            })
            .catch(console.error)
            .finally(() => setLoading(false))
    }, [open])

    const handleSpin = () => {
        if (mustSpin || wheelData.length < 2) return
        const newPrize = Math.floor(Math.random() * wheelData.length)
        setPrizeNumber(newPrize)
        setResult(null)
        setMustSpin(true)
    }

    const handleStopSpinning = async () => {
        setMustSpin(false)
        const landedOption = wheelData[prizeNumber]?.option ?? ''
        setResult(landedOption)

        setNotifying(true)
        try {
            await fetch(`${domainUrlPrefix()}/api/wheel/spin`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
                },
                body: JSON.stringify({ selectedUser: username, result: landedOption }),
            })
        } catch (err) {
            console.error('Failed to send spin notification:', err)
        } finally {
            setNotifying(false)
        }
    }

    return (
        <Dialog
            open={open}
            onClose={mustSpin ? undefined : onClose}
            fullScreen
            PaperProps={{ sx: { display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center' } }}
        >
            <DialogTitle sx={{ textAlign: 'center', fontSize: '1.8rem', fontWeight: 700 }}>
                Spin the Wheel
            </DialogTitle>
            <DialogContent sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 3, width: '100%', maxWidth: 600 }}>
                <Typography variant="subtitle1" color="text.secondary">
                    Spinning for: <strong>{username}</strong>
                </Typography>
                {loading ? (
                    <CircularProgress />
                ) : (
                    <>
                        {wheelData.length >= 2 ? (
                            <Box sx={{ display: 'flex', justifyContent: 'center' }}>
                                <Wheel
                                    mustStartSpinning={mustSpin}
                                    prizeNumber={prizeNumber}
                                    data={wheelData}
                                    onStopSpinning={handleStopSpinning}
                                    backgroundColors={SEGMENT_COLORS}
                                    textColors={['#ffffff']}
                                    fontSize={16}
                                    outerBorderColor="#333"
                                    outerBorderWidth={4}
                                    innerRadius={10}
                                    radiusLineColor="#ffffff"
                                    radiusLineWidth={1}
                                    spinDuration={0.8}
                                />
                            </Box>
                        ) : (
                            <Typography color="text.secondary">
                                No wheel options configured. Add some in the admin panel first.
                            </Typography>
                        )}

                        {result && !mustSpin && (
                            <Box sx={{ textAlign: 'center', mt: 1 }}>
                                <Typography variant="h5" fontWeight={700} color="primary">
                                    🎉 {result}
                                </Typography>
                                <Typography variant="body2" color="text.secondary">
                                    {username} spun the wheel and landed on &ldquo;{result}&rdquo;!
                                    {notifying && ' Notifying subscribers…'}
                                </Typography>
                            </Box>
                        )}

                        <Box sx={{ display: 'flex', gap: 2, mt: 1 }}>
                            <Button
                                variant="contained"
                                size="large"
                                onClick={handleSpin}
                                disabled={mustSpin || wheelData.length < 2}
                            >
                                {mustSpin ? 'Spinning…' : 'Spin!'}
                            </Button>
                            <Button
                                variant="outlined"
                                size="large"
                                onClick={onClose}
                                disabled={mustSpin}
                            >
                                Close
                            </Button>
                        </Box>
                    </>
                )}
            </DialogContent>
        </Dialog>
    )
}
