#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXAbsolutelyScheduledTimingType.h"
#include "FXRelativelyScheduledTimingType.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType FRAME-TIMINGS
 *
 * Time behavior of a frame specified in a sequence of cluster specific timing types
 */
class ASAM_FIBEX_EXPORT FXFrameTimings
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXFrameTimings(FXFibex *fibex = nullptr);
    ~FXFrameTimings();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @opt element RELATIVELY-SCHEDULED-TIMING */
    QList<FXRelativelyScheduledTimingType*> m_relativelyScheduledTimings;
    /** @opt element ABSOLUTELY-SCHEDULED-TIMING */
    QList<FXAbsolutelyScheduledTimingType*> m_absolutelyScheduledTimings;
};

} // FIBEX
} // ASAM
