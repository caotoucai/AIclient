#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FibexTypes.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

/**
 * @brief complexType flexray:WAKE-UP
 *
 * Wake-up related elements. If not used, then the device does not use wake up
 * functionality.
 *
 */
class ASAM_FIBEX_EXPORT FRWakeUp
{
public:
    FRWakeUp();
    ~FRWakeUp();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** element flexray:WAKE-UP-SYMBOL-RX-IDLE */
    quint16 m_wakeUpSymbolRxIdle = 14;
    /** element flexray:WAKE-UP-SYMBOL-RX-LOW */
    quint16 m_wakeUpSymbolRxLow = 10;
    /** element flexray:WAKE-UP-SYMBOL-RX-WINDOW */
    quint16 m_wakeUpSymbolRxWindow = 82;
    /** element flexray:WAKE-UP-SYMBOL-TX-IDLE */
    quint16 *m_wakeUpSymbolTxIdle = nullptr;
    /** element flexray:WAKE-UP-SYMBOL-TX-LOW */
    quint16 *m_wakeUpSymbolTxLow = nullptr;
};

} // FIBEX
} // ASAM
