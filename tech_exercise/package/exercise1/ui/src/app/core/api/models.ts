/** Every API response, success or error, shares this shape (D15). */
export interface BaseResponse {
  success: boolean;
  message: string;
  responseCode: number;
}

/** A person with their current astronaut details. Null fields mean they've never been an astronaut (RULE-2). */
export interface PersonAstronaut {
  personId: number;
  name: string;
  currentRank: string | null;
  currentDutyTitle: string | null;
  /** ISO date-time, e.g. "2018-01-15T00:00:00". */
  careerStartDate: string | null;
  careerEndDate: string | null;
}

export interface AstronautDuty {
  id: number;
  rank: string;
  dutyTitle: string;
  dutyStartDate: string;
  /** Null for the current duty (RULE-4). */
  dutyEndDate: string | null;
}

export interface GetPeopleResult extends BaseResponse {
  people: PersonAstronaut[];
}

export interface GetPersonResult extends BaseResponse {
  person: PersonAstronaut;
}

export interface GetAstronautDutiesResult extends BaseResponse {
  person: PersonAstronaut;
  /** Newest first. */
  astronautDuties: AstronautDuty[];
}

/** Returned by commands that create or rename something. */
export interface IdResult extends BaseResponse {
  id: number;
}

export interface NewDuty {
  name: string;
  rank: string;
  dutyTitle: string;
  /** ISO date, e.g. "2024-01-01". */
  dutyStartDate: string;
}
