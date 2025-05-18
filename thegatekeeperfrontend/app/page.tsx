"use client"
import StoreProvider from "./StoreProvider";
import * as signalR from '@microsoft/signalr';
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { useEffect, useMemo, useState } from "react";
import { CombinedContext, User } from "@/context/contextProvider";
import { AppConfigurationDtoV1, FrontEndInfo, GateKeeperAppInfo, setIsMobileDevice, setUsersOnline, updateAppConfig, updateUsersIfBlocked } from "@/store/features/baseComponentsSlice";
import { ThemeProvider } from "@mui/material";
import { ControlPanel } from "./components/controlPanel";
import { TheGateKeeper } from "./components/TheGateKeeperView";
import { lightTheme, darkTheme, vibrantTheme } from "@/context/themes";
import { fetchAllUsers, domainUrlPrefix, healthCheck, fetchConfiguration, adminAccess, fetchGateKeeperInfo } from "@/store/backEndCalls";
import { SimpleSnackbar } from "./components/snackBar";
import AdminControl from "./components/adminControl";

function App() {
  const dispatch = useAppDispatch()
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [user, setUser] = useState<User>({})
  const theme = useAppSelector(state => state.userSlice.selectedTheme)

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${domainUrlPrefix()}/backendUpdate`)
      .build();

    newConnection.on("ReceiveFrontEndInfo", (frontEndInfo: FrontEndInfo[]) => {
      dispatch(updateUsersIfBlocked(frontEndInfo))
    });
    newConnection.on("UpdateConfigurationView", (appConfiguration: AppConfigurationDtoV1) => {
      dispatch(updateAppConfig(appConfiguration))
    });
    newConnection.on("UsersOnline", (usersOnline: GateKeeperAppInfo) => {
      dispatch(setUsersOnline(usersOnline))
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
    if (window.innerWidth <= 768) {
      dispatch(setIsMobileDevice(true))
    }
    dispatch(healthCheck())
    dispatch(fetchConfiguration())
    dispatch(fetchAllUsers())
    dispatch(fetchGateKeeperInfo())
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
            vibrantTheme.palette.background.paper, minHeight: '98vh'
      }}>
        <CombinedContext.Provider value={value}>
          <ThemeProvider theme={theme === 'light' ? lightTheme :
            theme === 'dark' ? darkTheme :
              vibrantTheme}>
            <SimpleSnackbar />
            {adminAccess() && <AdminControl />}
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
