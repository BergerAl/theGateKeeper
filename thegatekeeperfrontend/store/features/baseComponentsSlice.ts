import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { fetchAllUsers, fetchConfiguration, fetchCurrentVoteStandings, fetchGateKeeperInfo, getUserHistory, healthCheck, updateConfiguration, voteForUser } from '../backEndCalls';

export enum SnackBarStatus {
    Ok,
    Warning,
    Error
}

export enum DisplayedView {
    DefaultPage = "DefaultPage",
    ResultsPage = "ResultsPage"
}

export interface SnackBarState {
    text: string,
    status?: SnackBarStatus
}

export interface Voting {
    isBlocked: boolean
    voteBlockedUntil: Date
}

export interface GateKeeperAppInfo {
    usersOnline: number
}

export interface VoteStandings {
    playerName: string
    votes: number
}

export interface GateKeeperInfo {
    name: string
}

export interface FrontEndInfo {
    name: string
    tier: string
    rank: string
    leaguePoints: number
    playedGames: number
    voting: Voting
}

export interface AppConfigurationDtoV1 {
    displayedView: DisplayedView
    votingDisabled: boolean
    displayResultsBar: boolean
}

export interface ViewState {
    actualSelectedPage: number
    snackBarState: SnackBarState
    chartView: {
        entry: RankTimeLineEntry
        visible: boolean
    },
    frontEndInfo: FrontEndInfo[]
    voteStandings: VoteStandings[]
    appConfiguration: AppConfigurationDtoV1
    gateKeeperInfo: GateKeeperInfo
    isDeviceMobile: boolean
    appInfo: GateKeeperAppInfo
}

export const initialState: ViewState = {
    actualSelectedPage: 0,
    snackBarState: { text: "", status: SnackBarStatus.Ok },
    chartView: {
        entry: {userName : "", rankTimeLine: []},
        visible: false
    },
    frontEndInfo: [],
    voteStandings: [],
    appConfiguration: { displayedView: DisplayedView.DefaultPage, votingDisabled: false, displayResultsBar: false },
    gateKeeperInfo: { name: '' },
    isDeviceMobile: false,
    appInfo: { usersOnline: 0 }
};

export interface RankTimeLineEntry {
    userName: string
    rankTimeLine: RankTimeLine[]
}

export interface RankTimeLine {
    dateTime: string
    rank: string
    leaguePoints: number
}

export const viewStateSlice = createSlice({
    name: 'viewState',
    initialState,
    reducers: {
        setSnackBarState: (state, action: PayloadAction<SnackBarState>) => {
            state.snackBarState = action.payload;
        },
        updateUsersIfBlocked: (state, action: PayloadAction<FrontEndInfo[]>) => {
            action.payload.forEach(element => {
                var playerIndex = state.frontEndInfo.findIndex(x => x.name == element.name)
                if (playerIndex !== -1) {
                    state.frontEndInfo[playerIndex].voting = element.voting
                }
            });
        },
        updateAppConfig: (state, action: PayloadAction<AppConfigurationDtoV1>) => {
            state.appConfiguration = action.payload
        },
        setIsMobileDevice: (state, action: PayloadAction<boolean>) => {
            state.isDeviceMobile = action.payload
        },
        setUsersOnline: (state, action: PayloadAction<GateKeeperAppInfo>) => {
            state.appInfo = action.payload
        },
        closeChartView: (state) => {
            state.chartView.visible = false;
            state.chartView.entry = { userName: "", rankTimeLine: [] };
        }
    },
    extraReducers(builder) {
        builder.addCase(fetchAllUsers.fulfilled, (state, action) => {
            state.frontEndInfo = action.payload;
        });
        builder.addCase(fetchAllUsers.rejected, (state, action) => {
            state.snackBarState = { text: action.error.message!, status: SnackBarStatus.Error };
        });
        builder.addCase(healthCheck.rejected, (state, action) => {
            state.snackBarState = { text: action.error.message!, status: SnackBarStatus.Error };
        });
        builder.addCase(voteForUser.fulfilled, (state, action) => {
            var playerIndex = state.frontEndInfo.findIndex(x => x.name == action.meta.arg)
            if (playerIndex !== -1) {
                state.frontEndInfo[playerIndex].voting.isBlocked = true
            }
            state.snackBarState = { text: `Voting was successful for ${action.meta.arg}`, status: SnackBarStatus.Ok };
        });
        builder.addCase(voteForUser.rejected, (state, action) => {
            state.snackBarState = { text: `Voting failed for ${action.meta.arg}`, status: SnackBarStatus.Error };
        });
        builder.addCase(fetchConfiguration.fulfilled, (state, action) => {
            state.appConfiguration = action.payload;
        });
        builder.addCase(fetchConfiguration.rejected, (state, action) => {
            state.appConfiguration = { displayedView: DisplayedView.DefaultPage, votingDisabled: true, displayResultsBar: false };
        });
        builder.addCase(updateConfiguration.fulfilled, (state, action) => {
            state.appConfiguration = action.meta.arg;
        });
        builder.addCase(updateConfiguration.rejected, (state, action) => {
            state.snackBarState = { text: `Setting state with value ${action.meta.arg} didn't work`, status: SnackBarStatus.Error };
        });
        builder.addCase(fetchCurrentVoteStandings.fulfilled, (state, action) => {
            state.voteStandings = action.payload
        });
        builder.addCase(fetchGateKeeperInfo.fulfilled, (state, action) => {
            state.gateKeeperInfo = action.payload
        });
        builder.addCase(getUserHistory.fulfilled, (state, action) => {
            state.chartView.entry = action.payload
            state.chartView.visible = true
        });
        builder.addCase(getUserHistory.rejected, (state, _) => {
            state.chartView.entry = {userName : "", rankTimeLine : []}
            state.chartView.visible = false
        });
    },
});

export const {
    setSnackBarState,
    updateUsersIfBlocked,
    updateAppConfig,
    setIsMobileDevice,
    setUsersOnline,
    closeChartView
} = viewStateSlice.actions;

export default viewStateSlice.reducer;
