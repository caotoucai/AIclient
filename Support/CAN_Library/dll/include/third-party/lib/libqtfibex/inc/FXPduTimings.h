#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXCyclicTimingType.h"
#include "FXEventControlledTimingType.h"
#include "FXRequestControlledTimingType.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType PDU-TIMINGS
 *
 * Time behavior of a PDU specified in a sequence of cluster specific timing types
 */
class ASAM_FIBEX_EXPORT FXPduTimings
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXPduTimings(FXFibex *fibex = nullptr);

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @todo element CYCLIC-TIMING */
    FXCyclicTimingType m_cyclicTiming;
    /** @todo element EVENT-CONTROLLED-TIMING */
    FXEventControlledTimingType m_eventControlledTiming;
    /** @todo element REQUEST-CONTROLLED-TIMING */
    FXRequestControlledTimingType m_requestControlledTiming;
};

} // FIBEX
} // ASAM

