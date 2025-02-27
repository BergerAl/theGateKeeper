
export enum CycleParameterEnum {
  WorkpieceLength = "WorkpieceLength",
  LengthYOverrun = "LengthYOverrun",
  FeedRate = "FeedRate",
  Base = "Base",
  DresserNumber = "DresserNumber",
  StartingXPosition = "StartingXPosition",
  ActivateContactRecognition = "ActivateContactRecognition",
  ContactRecognitionInfeed = "ContactRecognitionInfeed"
}

export type ParameterKeyPair = Record<string, string>;

export enum EditMode {
  None,
  NewAdded,
  Edit
}

export interface BackendRequest {
  status: 'success' | 'loading' | 'failed';
}