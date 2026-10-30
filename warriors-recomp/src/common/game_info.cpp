#include "app/app.h"

namespace warriors {
void StartApuClock();
}

// EUR retail — APU sample clock required before XACT touches FE80200C.
const app::GameInfo app::kGame = {
    .title = L"The Warriors",
    .short_title = L"The Warriors",
    .name = L"Warriors",
    .log = "warriors",
    .settings = "settings.toml",
    .splash = L"splash",
    .missing = L"Put the EUR game in the assets folder: its disc image (.iso), or default.xbe with its folders beside it.",
    .images = {{"default.xbe", "Warriors"}},
    .options = {},
    .load = nullptr,
    .save = nullptr,
    .init = nullptr,
    .loaded = warriors::StartApuClock,
    .window = nullptr,
    .message = nullptr,
    .ui = nullptr,
    .pad = nullptr,
    .rumble = nullptr,
};
