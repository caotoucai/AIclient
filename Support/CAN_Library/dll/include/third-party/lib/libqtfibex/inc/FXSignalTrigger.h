#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FibexTypes.h"
#include "FXRevisedElementType.h"

namespace ASAM {
namespace FIBEX {

/**
 * @brief complexType SIGNAL-TRIGGER
 *
 * Signal state that must rule that the referencing timing is executed.
 * To enable the description of combined tirggers the occurence of SIGNAL-TRIGGER as well as of SIGNAL-STATE combined with SIGNAL-INSTANCE-REF is unbounded.
 * Use one trigger with many states, if these states must rule simultaneously (AND related) for execution.
 * Use a separate trigger for each state if only one state needs to rule for execution.
 */
class ASAM_FIBEX_EXPORT FXSignalTrigger : public FXRevisedElementType
{
public:
    FXSignalTrigger();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** element fx:SIGNAL-STATE */
    QList<FibexTypes::FXExecuteConditionSignalStateType> m_signalStateList;
    /** @todo element fx:SIGNAL-INSTANCE-REF */
};

} // FIBEX
} // ASAM

