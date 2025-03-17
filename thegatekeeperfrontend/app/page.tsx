"use client"
import StoreProvider from "./StoreProvider";
import * as signalR from '@microsoft/signalr';
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { useEffect, useMemo, useState } from "react";
import { CombinedContext, User } from "@/context/contextProvider";
import { FrontEndInfo, updateUsersIfBlocked } from "@/store/features/baseComponentsSlice";
import { ThemeProvider } from "@mui/material";
import { ControlPanel } from "./components/controlPanel";
import { TheGateKeeper } from "./components/TheGateKeeperView";
import { lightTheme, darkTheme, vibrantTheme } from "@/context/themes";
import { fetchAllUsers } from "@/store/backEndCalls";
import { SimpleSnackbar } from "./components/snackBar";

function App() {
  const dispatch = useAppDispatch()
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [user, setUser] = useState<User>({})
  const theme = useAppSelector(state => state.controlPanelSlice.selectedTheme)

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl("/timedOutUserVote")
      .build();

    newConnection.on("ReceiveFrontEndInfo", (frontEndInfo: FrontEndInfo[]) => {
      dispatch(updateUsersIfBlocked(frontEndInfo))
    });

    newConnection.start().catch(err => console.error('Error starting connection:', err));

    setConnection(newConnection);

    return () => {
      if (connection) {
        connection.stop();
      }
    };
    // eslint-disable-next-line
  }, []);

  useEffect(() => {
    dispatch(fetchAllUsers())
    // eslint-disable-next-line
  }, []);

  const value = useMemo(() => ({
    user,
  }), [user])

  return (
    <html lang="en">
      <body style={{
        background: theme === 'light' ? lightTheme.palette.background.paper :
          theme === 'dark' ? darkTheme.palette.background.paper :
            vibrantTheme.palette.background.paper
      }}>
        <CombinedContext.Provider value={value}>
          <ThemeProvider theme={theme === 'light' ? lightTheme :
            theme === 'dark' ? darkTheme :
              vibrantTheme}>
            <SimpleSnackbar />
            <TheGateKeeper />
            <ControlPanel />
          </ThemeProvider>
        </CombinedContext.Provider>
      </body>
    </html>
  );
}

export default function Home() {
  return (
    <StoreProvider>
      <App />
    </StoreProvider>
  );
}
