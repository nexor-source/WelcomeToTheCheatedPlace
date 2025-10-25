# DarkPlaceRoomOverlay

一个用于《Welcome to the Dark Place》的房间信息可视化 BepInEx 插件。  
A BepInEx plugin that surfaces room navigation info for *Welcome to the Dark Place*.

> “迷宫里摸黑前进很浪费时间，不如直接看清楚自己要去的地方。”  
> “Wandering blind inside a maze wastes time—let the overlay show you where each choice leads.”

---

## 功能 / Features

- **当前位置提示**：始终在屏幕左上角显示当前房间与所属区域。  
  **Current room banner**: Always-visible banner in the top-left with room name and area.

- **悬停路径预览**：鼠标指向游戏内的选项按钮时，立刻显示该选择通向的目标房间与区域。  
  **Choice hover tooltip**: Hovering any in-game choice button reveals its destination room and area.

- **自适应字号**：根据屏幕高度自动缩放字号与留白，全屏后界面仍保持协调。  
  **Responsive sizing**: Fonts and padding scale with screen height, keeping the overlay proportional in fullscreen.

- **可开关**：默认 `F8` 切换显示（可在配置文件中修改）。  
  **Toggleable**: `F8` toggles visibility (rewritable via config).

---

## 安装 / Installation

1. 确保已安装 BepInEx（5.x）并可以成功加载插件。  
   Make sure BepInEx 5.x is installed and working.
2. 将编译好的 `DarkPlaceRoomOverlay.dll` 放进 `BepInEx/plugins/` 目录。  
   Drop the built `DarkPlaceRoomOverlay.dll` into `BepInEx/plugins/`.
3. 启动游戏即可看到覆盖层。  
   Launch the game to see the overlay in action.

---

## 配置 / Configuration

插件首次运行后，会在 `BepInEx/config/` 生成 `nexor.darkplace.roomoverlay.cfg`，包含：  
After the first run, `BepInEx/config/nexor.darkplace.roomoverlay.cfg` is generated with:

- `ToggleKey`：切换叠加层显示的快捷键（默认 `F8`）。  
  **ToggleKey**: Hotkey that toggles the overlay (default `F8`).



