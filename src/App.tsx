import { useEffect, useState, useMemo } from 'react';
import * as signalR from '@microsoft/signalr';
import './App.css';
import { useAppDispatch, useAppSelector } from './app/hooks';
import { TheGateKeeper } from './TheGateKeeperView';
import { ControlPanel } from './features/controlPanel/controlPanel';
import { CombinedContext, User } from './context/contextProvider';
import { selectCurrentThemeMode } from './features/controlPanel/controlPanelSlice';
import { ThemeProvider } from '@mui/material';
import { lightTheme, darkTheme, vibrantTheme } from './context/themes';
import { fetchAllUsers } from './features/baseComponents/baseComponentsSlice';

export const App: React.FC = () => {
  const dispatch = useAppDispatch()
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [user, setUser] = useState<User>({})
  const theme = useAppSelector(selectCurrentThemeMode)

  // useEffect(() => {
  //   const newConnection = new signalR.HubConnectionBuilder()
  //     .withUrl("http://localhost:8891/workpieceEvent")
  //     .build();

  //   // newConnection.on("ReceiveWorkpieceList", (workPieceFileList: WorkpieceStructure[]) => {
  //   //   dispatch(setAvailableWorkpieces(workPieceFileList))
  //   // });

  //   newConnection.start().catch(err => console.error('Error starting connection:', err));

  //   setConnection(newConnection);

  //   return () => {
  //     if (connection) {
  //       connection.stop();
  //     }
  //   };
  //   // eslint-disable-next-line
  // }, []);

  useEffect(() => {
    dispatch(fetchAllUsers())
    // eslint-disable-next-line
  }, []);

  const value = useMemo(() => ({
    user,
  }), [user])

  return (
    <CombinedContext.Provider value={value}>
      <ThemeProvider theme={theme === 'light' ? lightTheme :
        theme === 'dark' ? darkTheme :
          vibrantTheme}>
        <div style={{
          background: theme === 'light' ? lightTheme.palette.background.paper :
            theme === 'dark' ? darkTheme.palette.background.paper :
              vibrantTheme.palette.background.paper
        }}>
          <TheGateKeeper />
          <ControlPanel />
        </div>
      </ThemeProvider>
    </CombinedContext.Provider>
  );
}
