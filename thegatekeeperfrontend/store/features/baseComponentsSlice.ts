import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { fetchAllUsers, fetchConfiguration, fetchCurrentVotingStandings, fetchGateKeeperInfo, healthCheck, updateConfiguration, voteForUser } from '../backEndCalls';

export enum SnackBarStatus {
    Ok,
    Warning,
    Error
}

export enum ModalTypeDialog {
    None,
    WorkpieceCreation,
    DeleteWorkpiece
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

export interface VotingStandings {
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
    modalDialog: {
        modalDialogType: ModalTypeDialog
        visible: boolean
    },
    frontEndInfo: FrontEndInfo[],
    votingStandings: VotingStandings[]
    appConfiguration: AppConfigurationDtoV1,
    gateKeeperInfo: GateKeeperInfo
    isDeviceMobile: boolean
}

export const initialState: ViewState = {
    actualSelectedPage: 0,
    snackBarState: { text: "", status: SnackBarStatus.Ok },
    modalDialog: {
        modalDialogType: ModalTypeDialog.None,
        visible: false
    },
    frontEndInfo: [],
    votingStandings: [],
    appConfiguration: { displayedView: DisplayedView.DefaultPage, votingDisabled: false, displayResultsBar: false },
    gateKeeperInfo: { name: '' },
    isDeviceMobile: false
};

export const viewStateSlice = createSlice({
    name: 'viewState',
    initialState,
    reducers: {
        setSnackBarState: (state, action: PayloadAction<SnackBarState>) => {
            state.snackBarState = action.payload;
        },
        setModalDialogState: (state, action: PayloadAction<{ visible: boolean, modalDialogType?: ModalTypeDialog }>) => {
            state.modalDialog = { visible: action.payload.visible, modalDialogType: action.payload?.modalDialogType ? action.payload?.modalDialogType : ModalTypeDialog.None };
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
        builder.addCase(fetchCurrentVotingStandings.fulfilled, (state, action) => {
            state.votingStandings = action.payload
        });
        builder.addCase(fetchGateKeeperInfo.fulfilled, (state, action) => {
            state.gateKeeperInfo = action.payload
        });
    },
});

export const {
    setSnackBarState,
    setModalDialogState,
    updateUsersIfBlocked,
    updateAppConfig,
    setIsMobileDevice
} = viewStateSlice.actions;

export default viewStateSlice.reducer;
