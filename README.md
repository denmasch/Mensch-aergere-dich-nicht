# About
This repository was created as an Assignment for the University Course 'Advanced Software Engineering' at Duale Hochschule Karlsruhe. 
It is used to build a CLI-based Mulitplayer Board Game in Germany known as 'Mensch ärgere dich nicht'.


# Rules
This repository uses the official Rules set by 'Schmidt Spiele GmbH' for the Game 'Mensch ärgere dich nicht'. 
For the official Rules [click here](https://www.schmidtspiele.de/files/Produkte/4/49324%20-%20Reise%20Mensch%20%C3%A4rgere%20Dich%20nicht%C2%AE/49324_Mensch_aergere_Dich_nicht_REISE_DE.pdf).

# How to play
## Prerequisites
- Docker and Docker Compose installed on your machine.

## Steps to play
1. Download the last realease from the [releasepage]().
2. Unzip the downloaded file.
3. Open a terminal and navigate to the unzipped directory.
4. Start the game server using Docker Compose:
   ```bash
   docker compose up -d
   ```
5. Run the client application:
   ```bash
   MadnClient
   ```
6. After playing, you can stop the server using:
   ```bash
   docker compose down
   ```