#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FibexTypes.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

/**
 * @brief complexType SYSTEM-TRIGGER
 *
 * System state that must rule that the referencing timing is executed.
 * To enable the description of combined states the occurence of SYSTEM-TRIGGER as well as of SYSTEM-STATE is unbounded.
 * Use one trigger with many states, if these states must rule simultaneously (AND related) for execution.
 * Use a separate trigger for each state if only one state needs to rule for execution.
 */
class ASAM_FIBEX_EXPORT FXSystemTrigger
{
public:
    FXSystemTrigger();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @todo element SYSTEM-STATE */
    QList<FibexTypes::FXActiveConditionSystemStateType> m_systemStateList;
};

} // FIBEX
} // ASAM
