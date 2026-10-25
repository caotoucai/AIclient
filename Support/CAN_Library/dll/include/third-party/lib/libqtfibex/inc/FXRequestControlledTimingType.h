#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXTimeRangeType.h"
#include "FXActiveConditionType.h"
#include "FXExecuteConditionType.h"

namespace ASAM {
namespace FIBEX {

/**
 * @brief complexType REQUEST-CONTROLLED-TIMING-TYPE
 *
 * Content model for the entity TIMING in the request controlled peculiarity.
 */
class ASAM_FIBEX_EXPORT FXRequestControlledTimingType
{
public:
    FXRequestControlledTimingType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @todo element RESPONSE-TIME-RANGE */
    FXTimeRangeType m_responseTimeRange;
    /** @todo element ACTIVE-CONDITION */
    FXActiveConditionType m_activeCondition;
    /** @todo element FINAL-REPETITIONS */
    quint16 m_finalRepetitions;
    /** @todo element DESC */
    QString m_desc;
    /** @todo element MANUFACTURER-EXTENSION */
};

} // FIBEX
} // ASAM
