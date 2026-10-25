#ifndef SPXLUTILS_H
#define SPXLUTILS_H

#include <string>
#include <vector>
#include <algorithm>    // std::remove_if
#include <chrono>

using namespace std::chrono;

namespace sapa {

template<typename T, size_t N>
size_t ArraySize( T (&array)[N] )
{
    (void)array;
    return N;
}

template<typename T, int N>
T *ArrayBegin(T (&array)[N])
{
    return &array[0];
}

template<typename T, int N>
T *ArrayEnd(T (&array)[N])
{
    return &array[N-1];
}

template<typename T, int N>
const T *ArrayBeginConst(const T (&array)[N])
{
    return &array[0];
}

template<typename T, int N>
const T *ArrayEndConst(const T (&array)[N])
{
    return &array[N-1];
}

/*
template<typename T, typename P>
T remove_if(T beg, T end, P pred)
{
    T dest = beg;
    for (T itr = beg;itr != end; ++itr)
        if (!pred(*itr))
            *(dest++) = *itr;
    return dest;
}
*/

namespace utils {

std::vector<std::string> ulSplit(const char *phrase, std::string delimiter);
std::vector<uint8_t> hexString2Vector(const std::string &string);

void uint32ToUint8Vector(std::vector<uint8_t> &vec, uint32_t data);

uint64_t getTimeStampUs();
uint64_t getTimeStampMs();
std::string serializeTimePoint( const std::chrono::system_clock::time_point& time,
                                const std::string& format);

}

}
#endif // SPXLUTILS_H
