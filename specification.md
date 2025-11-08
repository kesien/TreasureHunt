# Treasure Hunt App

### Felhasználó típusok
- Admin
- Csapat

## Felhasználói felületek

### Admin nézetek
- Login
- Dashboard
- Események
    - Résztvevő csapatok
    - Progress
    - Képek
    - Helyszínek
- Regisztrációk

### Csoport nézetek
- Login (QR kód vagy Csapat kód)
- Regisztráció eseményre
- Aktuális esemény
  - Képek
- Dashboard

## Use casek

### Admin

#### Admin login
Van /admin/login page ahol egy login form jelenik meg és az admin felhasználónévvel és jelszóval betud jelentkezni és a sikeres bejelentkezés után az alkalmazás a dashboard page-re irányítja át.

#### Dashboard
A dashboardon kilistázzuk a pending regisztrációkat. Az aktív események számát. A totál események számát és mondjuk a korábbi eseményeket lista szerűen.

#### Esemény nézet
Az esemény nézeten az esemény részleteit jelenítjük meg. Mikor volt, kik vettek részt, lokációk, képek, stb.

#### Regisztrációk
A pending regisztrációk listázása (ki, mikor, mire). Részletek gomb (csapat lista), Engedélyez, Elutasítás

----

### Csapatok

#### Login
QR kód vagy csapat kód beírásával tud bejelentkezni és a sikeres bejelentkezés után az alkalmazás a dashboard pagere irányítja át.

#### Dashboard
A dashboardon az éppen zajló vagy a következő eseményt jelenítjük meg amelyen a csapat résztvesz. 