// MCPX APU global sample counter (NV_PAPU_XGSCNT @ 0xFE80200C).
// XACT reads this MMIO directly for timing; the stock host does not map FE80xxxx,
// so we commit that page and keep a 48 kHz counter ticking on a host thread.
#include <atomic>
#include <chrono>
#include <cstdint>
#include <thread>

#include "kernel/memory.h"

namespace warriors {

namespace {

constexpr uint32_t kSampleCounter = 0xFE80200C;
constexpr uint64_t kSampleRate = 48000;

}  // namespace

void StartApuClock() {
    const uint32_t page = kSampleCounter & ~(mem::kPage - 1);
    if (!mem::Commit(page, mem::kPage)) return;

    auto* counter = reinterpret_cast<std::atomic<uint32_t>*>(mem::Base() + kSampleCounter);
    counter->store(0, std::memory_order_relaxed);

    std::thread([counter] {
        const auto start = std::chrono::steady_clock::now();
        for (;;) {
            const auto us = std::chrono::duration_cast<std::chrono::microseconds>(
                std::chrono::steady_clock::now() - start);
            counter->store(static_cast<uint32_t>(static_cast<uint64_t>(us.count()) * kSampleRate / 1000000ull),
                           std::memory_order_relaxed);
            std::this_thread::sleep_for(std::chrono::milliseconds(1));
        }
    }).detach();
}

}  // namespace warriors
