#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXTimeRangeType.h"
#include "FXActiveConditionType.h"
#include "FXExecuteConditionType.h"

namespace ASAM {
namespace FIBEX {

/**
 * @brief complexType EVENT-CONTROLLED-TIMING-TYPE
 *
 * Content model for the entity TIMING in the event controlled peculiarity.
 */
class ASAM_FIBEX_EXPORT FXEventControlledTimingType
{
public:
    FXEventControlledTimingType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** element DEBOUNCE-TIME-RANGE */
    FXTimeRangeType m_debounceTimeRange;
    /** element ACTIVE-CONDITION */
    FXActiveConditionType m_activeCondition;
    /** element SEND-CONDITION */
    FXExecuteConditionType m_sendCondition;
    /** element FINAL-REPETITIONS */
    quint16 m_finalRepetitions;
    /** element DESC */
    QString m_desc;
    /** @todo element MANUFACTURER-EXTENSION */
};

} // FIBEX
} // ASAM
