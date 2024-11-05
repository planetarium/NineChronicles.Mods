# Illusionist

Illusionist help you transform assets like images and audio into whatever you want them to be.

## Key Features

- Weapon image replacement: The Illusionist allows players to change the appearance of a weapon to a desired style.

## Install and run

### Install: Windows

1. Download the latest version of the Illusionist mod compressed file from [GitHub release](https://github.com/planetarium/NineChronicles.Mods/releases): `Illusionist-x.y.z.zip`.
2. Press the `Windows + R` keys simultaneously to open the Run dialog box, type `%USERPROFILE%\AppData\Roaming\Nine Chronicles\player\main` and press `Enter`.
3. In the folder that opens, unzip the downloaded file and paste the contents inside the `Illusionist-x.y.z` folder.

### Run

Illusionist will run automatically without any conditions.

> [!TIP]
> If the game doesn't run normally or mods don't activate, double-check the installation process and make sure all files are in the right places.

## Replace weapon image

### Select target weapon

To replace a weapon, you need the weapon's sheet ID. To find it, you can use the `9c-board` service:
- https://9c-board.nine-chronicles.dev/{planet-name}/avatar/{address}
    - You can use the following values for `planet-name`:
        - odin
        - heimdall
    - For `address`, enter the address of your avatar.

For example, an avatar at the address `0xe56d432da2032f6C851943b76C4a41815baaBB54` on the planet `odin` might have the following URL
- `https://9c-board.nine-chronicles.dev/odin/avatar/0xe56d432da2032f6C851943b76C4a41815baaBB54`

Hover over the desired weapon image to see the weapon's sheet ID.

![image](https://github.com/user-attachments/assets/48f471a2-4b24-43f7-baac-91ee21781da2)

### Change weapon image

To replace a weapon image file, save the desired weapon image file to the path below:
- `%USERPROFILE%\AppData\Roaming\Nine Chronicles\player\main\BepInEx\plugins\Illusionist\CharacterTextures\Weapons\{weapon's sheet ID}.png`

For this example, we will use `10100000`, which is the sheet ID of the wooden stick used when the weapon is unsheathed. Save the desired weapon image file as `10100000.png` in the path below, and it will change the appearance of that weapon in-game:
- `%USERPROFILE%\AppData\Roaming\Nine Chronicles\player\main\BepInEx\plugins\Illusionist\CharacterTextures\Weapons\10100000.png`

This is the weapon image I used.

![image](https://github.com/user-attachments/assets/66a2ff15-ca9c-4e91-98ca-c285058f1499)

Now run the game, unequip the weapon, and you will see the new image instead of the default weapon, a wooden stick.

<img width="273" alt="image" src="https://github.com/user-attachments/assets/a635b79b-296f-4c2d-8b99-6d5a39861564">
<img width="670" alt="image" src="https://github.com/user-attachments/assets/fa5bb6a9-b6cd-46bd-a9b2-b97a5ac0e220">

