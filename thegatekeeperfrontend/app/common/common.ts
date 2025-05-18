export function translateUsersOnline(userOnline: number) {
  if (userOnline === 1) {
    return `There is currently ${userOnline} clown online`
  } else if (userOnline > 1) {
    return `There are currently ${userOnline} clowns online`
  } else {
    return ``
  }
}