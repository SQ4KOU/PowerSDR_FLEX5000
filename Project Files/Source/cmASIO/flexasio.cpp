// FLEX-5000 raw FireWire/ASIO transport for Thetis.
// Dedicated 8x8 Float32 path at 192 kHz.  This intentionally does not use
// the existing 2x2/48 kHz cmASIO audio path.

#include <windows.h>
#include <string.h>
#include "asiosys.h"
#include "asio.h"
#include "asiodrivers.h"
#include "flexasio.h"

extern AsioDrivers* asioDrivers;
bool loadAsioDriver(char* name);

namespace
{
    const int kChannels = 8;

    struct FlexDriverState
    {
        ASIODriverInfo driverInfo;
        ASIOBufferInfo bufferInfos[kChannels * 2];
        ASIOChannelInfo channelInfos[kChannels * 2];
        ASIOCallbacks callbacks;
        long inputChannels;
        long outputChannels;
        long minSize;
        long maxSize;
        long preferredSize;
        long granularity;
        long blockSize;
        ASIOSampleRate sampleRate;
        FlexASIOCallback callback;
        bool loaded;
        bool buffersCreated;
        bool started;
        int lastError;
    };

    FlexDriverState g = {};

    void setError(int e)
    {
        g.lastError = e;
    }

    void zeroOutputs(long doubleBufferIndex)
    {
        if (g.blockSize <= 0) return;
        for (int i = 0; i < kChannels; i++)
        {
            void* p = g.bufferInfos[kChannels + i].buffers[doubleBufferIndex];
            if (p) memset(p, 0, (size_t)g.blockSize * sizeof(float));
        }
    }

    ASIOTime* bufferSwitchTimeInfo(ASIOTime* params, long doubleBufferIndex, ASIOBool directProcess)
    {
        (void)directProcess;
        if (!g.buffersCreated || doubleBufferIndex < 0 || doubleBufferIndex > 1)
            return params;

        zeroOutputs(doubleBufferIndex);

        if (g.callback)
        {
            g.callback(
                g.bufferInfos[0].buffers[doubleBufferIndex],
                g.bufferInfos[1].buffers[doubleBufferIndex],
                g.bufferInfos[2].buffers[doubleBufferIndex],
                g.bufferInfos[3].buffers[doubleBufferIndex],
                g.bufferInfos[4].buffers[doubleBufferIndex],
                g.bufferInfos[5].buffers[doubleBufferIndex],
                g.bufferInfos[6].buffers[doubleBufferIndex],
                g.bufferInfos[7].buffers[doubleBufferIndex],
                g.bufferInfos[8].buffers[doubleBufferIndex],
                g.bufferInfos[9].buffers[doubleBufferIndex],
                g.bufferInfos[10].buffers[doubleBufferIndex],
                g.bufferInfos[11].buffers[doubleBufferIndex],
                g.bufferInfos[12].buffers[doubleBufferIndex],
                g.bufferInfos[13].buffers[doubleBufferIndex],
                g.bufferInfos[14].buffers[doubleBufferIndex],
                g.bufferInfos[15].buffers[doubleBufferIndex],
                (int)g.blockSize);
        }

        ASIOOutputReady();
        return params;
    }

    void bufferSwitch(long doubleBufferIndex, ASIOBool directProcess)
    {
        ASIOTime timeInfo = {};
        bufferSwitchTimeInfo(&timeInfo, doubleBufferIndex, directProcess);
    }

    void sampleRateDidChange(ASIOSampleRate sRate)
    {
        g.sampleRate = sRate;
    }

    long asioMessages(long selector, long value, void* message, double* opt)
    {
        (void)value;
        (void)message;
        (void)opt;

        switch (selector)
        {
        case kAsioSelectorSupported:
            switch (value)
            {
            case kAsioResetRequest:
            case kAsioEngineVersion:
            case kAsioResyncRequest:
            case kAsioLatenciesChanged:
            case kAsioSupportsTimeInfo:
            case kAsioSupportsTimeCode:
                return 1L;
            default:
                return 0L;
            }
        case kAsioResetRequest:
        case kAsioResyncRequest:
        case kAsioLatenciesChanged:
            return 1L;
        case kAsioEngineVersion:
            return 2L;
        case kAsioSupportsTimeInfo:
            return 1L;
        case kAsioSupportsTimeCode:
            return 0L;
        default:
            return 0L;
        }
    }

    void cleanup()
    {
        if (g.started)
        {
            ASIOStop();
            g.started = false;
        }
        if (g.buffersCreated)
        {
            ASIODisposeBuffers();
            g.buffersCreated = false;
        }
        if (g.loaded)
        {
            ASIOExit();
            if (asioDrivers) asioDrivers->removeCurrentDriver();
            g.loaded = false;
        }
        g.callback = nullptr;
        g.blockSize = 0;
    }
}

extern "C" __declspec(dllexport) int prepareFlexASIO(int samplerate, const char* asioDriverName, FlexASIOCallback callback)
{
    cleanup();
    memset(&g, 0, sizeof(g));

    if (!asioDriverName || !asioDriverName[0] || !callback)
    {
        setError(-1);
        return -1;
    }

    char driverName[128] = {};
    strncpy_s(driverName, sizeof(driverName), asioDriverName, _TRUNCATE);
    if (!loadAsioDriver(driverName))
    {
        setError(-2);
        return -2;
    }
    g.loaded = true;
    g.callback = callback;

    g.driverInfo.asioVersion = 2;
    g.driverInfo.sysRef = GetDesktopWindow();
    if (ASIOInit(&g.driverInfo) != ASE_OK)
    {
        setError(-3);
        cleanup();
        return -3;
    }

    if (ASIOGetChannels(&g.inputChannels, &g.outputChannels) != ASE_OK ||
        g.inputChannels < kChannels || g.outputChannels < kChannels)
    {
        setError(-4);
        cleanup();
        return -4;
    }

    if (ASIOGetBufferSize(&g.minSize, &g.maxSize, &g.preferredSize, &g.granularity) != ASE_OK ||
        g.preferredSize <= 0)
    {
        setError(-5);
        cleanup();
        return -5;
    }
    g.blockSize = g.preferredSize;

    g.sampleRate = (ASIOSampleRate)samplerate;
    ASIOError rateCheck = ASIOCanSampleRate(g.sampleRate);
    if (rateCheck != ASE_OK)
    {
        setError(-6);
        cleanup();
        return -6;
    }
    if (ASIOSetSampleRate(g.sampleRate) != ASE_OK)
    {
        setError(-7);
        cleanup();
        return -7;
    }

    for (int i = 0; i < kChannels; i++)
    {
        g.bufferInfos[i].isInput = ASIOTrue;
        g.bufferInfos[i].channelNum = i;
        g.bufferInfos[i].buffers[0] = g.bufferInfos[i].buffers[1] = nullptr;

        g.bufferInfos[kChannels + i].isInput = ASIOFalse;
        g.bufferInfos[kChannels + i].channelNum = i;
        g.bufferInfos[kChannels + i].buffers[0] = g.bufferInfos[kChannels + i].buffers[1] = nullptr;
    }

    g.callbacks.bufferSwitch = &bufferSwitch;
    g.callbacks.sampleRateDidChange = &sampleRateDidChange;
    g.callbacks.asioMessage = &asioMessages;
    g.callbacks.bufferSwitchTimeInfo = &bufferSwitchTimeInfo;

    if (ASIOCreateBuffers(g.bufferInfos, kChannels * 2, g.blockSize, &g.callbacks) != ASE_OK)
    {
        setError(-8);
        cleanup();
        return -8;
    }
    g.buffersCreated = true;

    for (int i = 0; i < kChannels * 2; i++)
    {
        memset(&g.channelInfos[i], 0, sizeof(ASIOChannelInfo));
        g.channelInfos[i].channel = g.bufferInfos[i].channelNum;
        g.channelInfos[i].isInput = g.bufferInfos[i].isInput;
        if (ASIOGetChannelInfo(&g.channelInfos[i]) != ASE_OK ||
            g.channelInfos[i].type != ASIOSTFloat32LSB)
        {
            setError(-9);
            cleanup();
            return -9;
        }
    }

    setError(0);
    return 0;
}

extern "C" __declspec(dllexport) long flexAsioStart()
{
    if (!g.loaded || !g.buffersCreated) return ASE_NotPresent;
    ASIOError e = ASIOStart();
    if (e == ASE_OK) g.started = true;
    else setError(-10);
    return e;
}

extern "C" __declspec(dllexport) long flexAsioStop()
{
    if (!g.started) return ASE_OK;
    ASIOError e = ASIOStop();
    g.started = false;
    return e;
}

extern "C" __declspec(dllexport) void unloadFlexASIO()
{
    cleanup();
}

extern "C" __declspec(dllexport) int getFlexASIOBlockSize()
{
    return (int)g.blockSize;
}

extern "C" __declspec(dllexport) int getFlexASIOError()
{
    return g.lastError;
}
