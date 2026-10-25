#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXTimeRangeType.h"
#include "FXActiveConditionType.h"
#include "FXExecuteConditionType.h"

namespace ASAM {
namespace FIBEX {

/**
 * @brief complexType CYCLIC-TIMING-TYPE
 *
 * Content model for the entity TIMING in the cyclic peculiarity.
 */
class ASAM_FIBEX_EXPORT FXCyclicTimingType
{
public:
    FXCyclicTimingType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @todo element REPEATING-TIME-RANGE */
    FXTimeRangeType m_repeatingTimeRange;
    /** @todo element STARTING-TIME-RANGE */
    FXTimeRangeType m_startingTimeRange;
    /** @todo element ACTIVE-CONDITION */
    FXActiveConditionType m_activeCondition;
    /** @todo element START-CONDITION */
    FXExecuteConditionType m_startCondition;
    /** @todo element STOP-CONDITION */
    FXExecuteConditionType m_stopCondition;
    /** @todo element FINAL-REPETITIONS */
    quint16 m_finalRepetitions;
    /** @todo element DESC */
    QString m_desc;
    /** @todo element MANUFACTURER-EXTENSION */
};

} // FIBEX
} // ASAM
