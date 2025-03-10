"use client"
import StoreProvider from "./StoreProvider";
import * as signalR from '@microsoft/signalr';
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { useEffect, useMemo, useState } from "react";
import { CombinedContext, User } from "@/context/contextProvider";
import { fetchAllUsers } from "@/store/features/baseComponentsSlice";
import { ThemeProvider } from "@mui/material";
import { ControlPanel } from "./components/controlPanel";
import { TheGateKeeper } from "./components/TheGateKeeperView";
import { lightTheme, darkTheme, vibrantTheme } from "@/context/themes";

function App() {
  const dispatch = useAppDispatch()
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [user, setUser] = useState<User>({})
  const theme = useAppSelector(state => state.controlPanelSlice.selectedTheme)

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

export default function Home() {
  return (
    <StoreProvider>
      <App />
    </StoreProvider>
  );
}
